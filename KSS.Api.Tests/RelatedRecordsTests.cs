using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using AutoMapper;
using KSS.Api.Controller;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;
using KSS.Service.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KSS.Api.Tests
{
    /// <summary>Records every call made to a service and its arguments; returns empty, successful results.</summary>
    public class RecordingService : DispatchProxy
    {
        public List<string> Calls { get; } = new();
        public List<object?[]> Arguments { get; } = new();

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            Calls.Add(method!.Name);
            Arguments.Add(args ?? Array.Empty<object?>());
            var type = method.ReturnType;
            if (type == typeof(void)) return null;
            if (type == typeof(Task)) return Task.CompletedTask;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var inner = type.GetGenericArguments()[0];
                return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(inner).Invoke(null, new[] { Empty(inner) });
            }
            return Empty(type);
        }

        private static object? Empty(Type type)
        {
            if (type == typeof(ServiceResult)) return ServiceResult.Ok();
            if (type.IsValueType) return Activator.CreateInstance(type);
            return type.IsClass && type != typeof(string) && type.GetConstructor(Type.EmptyTypes) != null ? Activator.CreateInstance(type) : null;
        }
    }

    /// <summary>The same recorder, standing in for a service that declares company-scoped writes.</summary>
    public class ScopedRecordingService : RecordingService, ICompanyScopedWrites
    {
    }

    /// <summary>Copies same-named properties from source to a new destination instance.</summary>
    public class CopyMapper : DispatchProxy
    {
        public static IMapper Make() => Create<IMapper, CopyMapper>();

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == "Map" && method.IsGenericMethod && args!.Length == 1 && args[0] != null)
            {
                var destination = Activator.CreateInstance(method.ReturnType)!;
                foreach (var source in args[0]!.GetType().GetProperties().Where(p => p.CanRead))
                {
                    var target = method.ReturnType.GetProperty(source.Name);
                    if (target != null && target.CanWrite && target.PropertyType.IsAssignableFrom(source.PropertyType))
                        target.SetValue(destination, source.GetValue(args[0]));
                }
                return destination;
            }
            throw new NotSupportedException("Unexpected mapper call: " + method.Name);
        }
    }

    /// <summary>
    /// The generic write actions of BaseController. Two rules, one place:
    /// (1) a write is available only for a service that declares company-scoped writes, and
    /// (2) a write request carries the record itself, never a related record (the data layer
    /// would save related records too: updating, inserting, or removing them by cascade, and
    /// moving them between companies).
    /// </summary>
    public class RelatedRecordsTests
    {
        private static readonly Guid Own = Guid.Parse("aaaaaaaa-1111-0000-0000-000000000001");
        private static readonly Guid Other = Guid.Parse("bbbbbbbb-1111-0000-0000-000000000002");
        private static readonly Guid OtherRow = Guid.Parse("cccccccc-1111-0000-0000-000000000003");

        // The services expected to declare company-scoped writes. Listed by hand on purpose:
        // adding or removing a declaration must show up here.
        private static readonly string[] ScopedControllers = { "CompanyDocument", "FinancialInfo", "NameHistory", "NameHistoryTranslation", "Translation" };

        private static readonly string[] EntityWrites = { "AddAsync", "AddRangeAsync", "Update", "UpdateRange", "Remove", "RemoveRange" };
        private static readonly string[] AllWrites = { "AddAsync", "AddDtoAsync", "AddRangeAsync", "Update", "UpdateDto", "UpdateRange", "Remove", "RemoveRange" };

        private static bool IsRefusal(object? result) => result is ObjectResult { StatusCode: 400 or 403 };

        private static string Outcome(object? result) => result switch
        {
            Exception e => DatabaseSentinel.Reached(e) ? "database reached" : e.GetType().Name + ": " + e.Message,
            ObjectResult o => "status " + o.StatusCode,
            null => "null",
            _ => result.GetType().Name,
        };

        // ── The real stack: real controllers, services and repositories, database guarded ──

        private sealed class Stack
        {
            public DatabaseSentinel Sentinel { get; } = new();
            public MainDbContext Db { get; }
            public FakeAccessService Access { get; }
            private readonly IServiceProvider _provider;

            // A stored row, as if loaded earlier in the request, so a lookup by key needs no query.
            public T Stored<T>(T row) where T : class
            {
                Db.Attach(row);
                return row;
            }

            public Stack()
            {
                var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("personId", Guid.NewGuid().ToString()) }, "test")) } };
                Db = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>()
                    .UseSqlServer("Server=database.not.available.invalid;Database=none;Encrypt=False")
                    .AddInterceptors(Sentinel).Options, accessor);
                var (accessService, access) = FakeAccessService.Create();
                access.InformationLevels[Own] = 2;
                Access = access;

                var s = new ServiceCollection();
                s.AddSingleton(Db);
                s.AddSingleton(accessService);
                s.AddSingleton<IHttpContextAccessor>(accessor);
                s.AddSingleton(CopyMapper.Make());
                s.AddSingleton<ICurrentCompany>(new CurrentCompany(accessor));
                // Every repository interface resolves to its implementation on this context.
                foreach (var implementation in typeof(KSS.Repository.Repository.CompanyDocumentRepository).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository", StringComparison.Ordinal)))
                    foreach (var contract in implementation.GetInterfaces().Where(i => i.Name == "I" + implementation.Name))
                        s.AddSingleton(contract, p => ActivatorUtilities.CreateInstance(p, implementation));
                s.AddSingleton<ICompanyOwnershipService>(p => ActivatorUtilities.CreateInstance<CompanyOwnershipService>(p));
                _provider = s.BuildServiceProvider();
            }

            public object Controller(string name)
            {
                var type = BaseControllers().Single(t => t.Name == name + "Controller");
                var contract = type.GetConstructors().Single().GetParameters().Single().ParameterType;
                var implementation = typeof(CompanyService).Assembly.GetTypes().Single(t => t.IsClass && !t.IsAbstract && contract.IsAssignableFrom(t));
                return Activator.CreateInstance(type, ActivatorUtilities.CreateInstance(_provider, implementation))!;
            }

            public List<string> Tracked() => Db.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Select(e => e.State + " " + e.Metadata.ClrType.Name).ToList();
        }

        private static async Task<object?> Call(Func<object?> action)
        {
            try
            {
                var result = action();
                if (result is Task task)
                {
                    await task;
                    result = task.GetType().GetProperty("Result")?.GetValue(task);
                }
                return result is IConvertToActionResult convertible ? convertible.Convert() : result;
            }
            catch (Exception e) { return e is TargetInvocationException t ? t.InnerException : e; }
        }

        private static void AssertRefusedAndNothingWritten(string name, Stack s, object? result)
        {
            Assert.True(IsRefusal(result), name + " was not refused; outcome: " + Outcome(result));
            Assert.Empty(s.Tracked());
            Assert.Equal(0, s.Sentinel.Attempts);
        }

        private static Company CompanyRow(Guid id) => new() { Id = id, NationalId = "0000000000", RegistrationNo = "1", IsActive = true };

        // ── Nested related records: the measured cases ─────────────────────────────

        [Fact]
        public async Task Case_a_Company_Update_carrying_another_companys_name_entry()
        {
            var s = new Stack();
            var graph = CompanyRow(Own);
            graph.NameHistories.Add(new NameHistory { Id = OtherRow, CompanyId = Other, StartDate = new DateTime(2020, 1, 1) });
            var controller = (CompanyController)s.Controller("Company");
            AssertRefusedAndNothingWritten("Company.Update + nested NameHistory", s, await Call(() => controller.Update(graph)));
        }

        [Fact]
        public async Task Case_b_Address_Update_carrying_another_company()
        {
            var s = new Stack();
            var controller = (AddressController)s.Controller("Address");
            AssertRefusedAndNothingWritten("Address.Update + nested Company", s,
                await Call(() => controller.Update(new Address { Id = Guid.NewGuid(), CompanyId = Own, PostalCode = "1", Company = CompanyRow(Other) })));
        }

        [Fact]
        public async Task Case_c_Company_Remove_carrying_another_companys_address()
        {
            var s = new Stack();
            var graph = CompanyRow(Own);
            graph.Addresses.Add(new Address { Id = OtherRow, CompanyId = Other, PostalCode = "1" });
            var controller = (CompanyController)s.Controller("Company");
            AssertRefusedAndNothingWritten("Company.Remove + nested Address (cascade)", s, await Call(() => controller.Remove(graph)));
        }

        [Fact]
        public async Task Case_d_Company_Add_carrying_another_companys_name_entry()
        {
            var s = new Stack();
            var graph = CompanyRow(Guid.NewGuid());
            graph.NameHistories.Add(new NameHistory { Id = OtherRow, CompanyId = Other, StartDate = new DateTime(2020, 1, 1) });
            var controller = (CompanyController)s.Controller("Company");
            AssertRefusedAndNothingWritten("Company.AddAsync + nested NameHistory", s, await Call(() => controller.AddAsync(graph)));
        }

        [Fact]
        public async Task Case_f_FinancialInfo_Add_carrying_a_company()
        {
            var s = new Stack();
            var controller = (FinancialInfoController)s.Controller("FinancialInfo");
            AssertRefusedAndNothingWritten("FinancialInfo.AddAsync + nested Company", s,
                await Call(() => controller.AddAsync(new FinancialInfo { Id = Guid.NewGuid(), CompanyId = Own, FiscalYear = 1402, RegisteredCapital = 1, NumberOfShares = 1, Company = CompanyRow(Other) })));
        }

        // ── Direct writes, no nested record, to another company's rows (and reference data) ──

        public static IEnumerable<object[]> DirectWrites() => new[]
        {
            new object[] { "Address" }, new object[] { "AddressTranslation" }, new object[] { "Company" },
            new object[] { "Email" }, new object[] { "Phone" }, new object[] { "Software" },
            new object[] { "Stakeholder" }, new object[] { "StakeholderHistory" }, new object[] { "Website" },
            new object[] { "AddressLabel" },
        };

        private static object RowOfAnotherCompany(string name) => name switch
        {
            "Address" => new Address { Id = OtherRow, CompanyId = Other, PostalCode = "1" },
            "AddressTranslation" => new AddressTranslation { AddressId = OtherRow, LanguageId = 12, Street1 = "x" },
            "Company" => CompanyRow(Other),
            "Email" => new Email { Id = OtherRow, CompanyId = Other, EmailAddress = "x@example.test" },
            "Phone" => new Phone { Id = OtherRow, CompanyId = Other, PhoneNumber = "+989121234567" },
            "Software" => new Software { Id = 7, CompanyId = Other, Name = "x" },
            "Stakeholder" => new Stakeholder { Id = OtherRow, CompanyId = Other, RelatedPartyType = 2, RelatedPartyId = Guid.NewGuid(), StakeholderTypeId = 1 },
            "StakeholderHistory" => new StakeholderHistory { Id = OtherRow, CompanyStakeholderId = Guid.NewGuid(), OwnershipPercentage = 10, ShareCount = 1, RegistrationDate = new DateTime(2020, 1, 1), EffectiveDate = new DateTime(2020, 1, 1) },
            "Website" => new Website { Id = OtherRow, CompanyId = Other, Url = "https://example.test" },
            "AddressLabel" => new AddressLabel { Id = 3 },
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };

        [Theory]
        [MemberData(nameof(DirectWrites))]
        public async Task A_direct_write_to_another_companys_row_is_refused(string controller)
        {
            var s = new Stack();
            var instance = s.Controller(controller);
            var row = RowOfAnotherCompany(controller);

            var result = await Call(() => instance.GetType().GetMethod("Update")!.Invoke(instance, new[] { row }));

            Assert.True(IsRefusal(result), $"{controller}.Update of another company's row was not refused; outcome: {Outcome(result)}; tracked: {string.Join(",", s.Tracked())}");
            Assert.Empty(s.Tracked());
            Assert.Equal(0, s.Sentinel.Attempts);
        }

        [Fact]
        public async Task A_direct_add_by_dto_for_another_company_is_refused()
        {
            var s = new Stack();
            var controller = (AddressController)s.Controller("Address");

            var result = await Call(() => controller.AddDtoAsync(new AddressInsertDto { CompanyId = Other, LabelId = 1, CountryId = 1, RegionId = 1, CityId = 1, PostalCode = "1" }));

            Assert.True(IsRefusal(result), $"Address.AddDtoAsync for another company was not refused; outcome: {Outcome(result)}; tracked: {string.Join(",", s.Tracked())}");
            Assert.Empty(s.Tracked());
            Assert.Equal(0, s.Sentinel.Attempts);
        }

        [Fact]
        public async Task A_direct_update_by_dto_of_another_companys_row_is_refused()
        {
            var s = new Stack();
            var controller = (AddressController)s.Controller("Address");

            var result = await Call(() => controller.UpdateDto(new AddressDto { Id = OtherRow, CompanyId = Other, LabelId = 1, CountryId = 1, RegionId = 1, CityId = 1, PostalCode = "1" }));

            Assert.True(IsRefusal(result), $"Address.UpdateDto of another company's row was not refused; outcome: {Outcome(result)}; tracked: {string.Join(",", s.Tracked())}");
            Assert.Empty(s.Tracked());
            Assert.Equal(0, s.Sentinel.Attempts);
        }

        // ── The inherited writes the web applications call, in the shapes they send ──
        // Each lands on a service that declares company-scoped writes, so each must still
        // reach the service, pass its company check for a permitted caller, and reach the save.

        private static void AssertReachedTheSave(string name, Stack s, object? result, string expectedWrite)
        {
            Assert.False(IsRefusal(result), name + " was refused: " + Outcome(result));
            Assert.True(result is Exception e && DatabaseSentinel.Reached(e), name + " did not reach the save; outcome: " + Outcome(result));
            Assert.Contains(s.Access.LevelQueries, q => q.CompanyId == Own);
            Assert.Contains(expectedWrite, s.Tracked());
        }

        [Fact]
        public async Task Web_shape_FinancialInfo_Remove_with_the_five_field_projection_goes_through()
        {
            var s = new Stack();
            var id = Guid.NewGuid();
            s.Stored(new FinancialInfo { Id = id, CompanyId = Own, FiscalYear = 1402, RegisteredCapital = 1000, NumberOfShares = 10 });
            var controller = (FinancialInfoController)s.Controller("FinancialInfo");

            var result = await Call(() => controller.Remove(new FinancialInfo { Id = id, CompanyId = Own, FiscalYear = 1402, RegisteredCapital = 1000, NumberOfShares = 10 }));

            AssertReachedTheSave("FinancialInfo.Remove", s, result, "Deleted FinancialInfo");
        }

        [Fact]
        public async Task Web_shape_FinancialInfo_AddDto_goes_through()
        {
            var s = new Stack();
            var controller = (FinancialInfoController)s.Controller("FinancialInfo");

            var result = await Call(() => controller.AddDtoAsync(new FinancialInfoInsertDto { CompanyId = Own, FiscalYear = 1403, RegisteredCapital = 1000, NumberOfShares = 10 }));

            AssertReachedTheSave("FinancialInfo.AddDtoAsync", s, result, "Added FinancialInfo");
        }

        [Fact]
        public async Task Web_shape_FinancialInfo_UpdateDto_goes_through()
        {
            var s = new Stack();
            var id = Guid.NewGuid();
            s.Stored(new FinancialInfo { Id = id, CompanyId = Own, FiscalYear = 1402, RegisteredCapital = 1000, NumberOfShares = 10 });
            var controller = (FinancialInfoController)s.Controller("FinancialInfo");

            var result = await Call(() => controller.UpdateDto(new FinancialInfoDto { Id = id, CompanyId = Own, FiscalYear = 1402, RegisteredCapital = 2000, NumberOfShares = 10 }));

            AssertReachedTheSave("FinancialInfo.UpdateDto", s, result, "Modified FinancialInfo");
        }

        [Fact]
        public async Task Web_shape_NameHistory_UpdateDto_goes_through()
        {
            var s = new Stack();
            var id = Guid.NewGuid();
            s.Stored(new NameHistory { Id = id, CompanyId = Own, StartDate = new DateTime(2020, 1, 1) });
            var controller = (NameHistoryController)s.Controller("NameHistory");

            var result = await Call(() => controller.UpdateDto(new NameHistoryDto { Id = id, CompanyId = Own, StartDate = new DateTime(2020, 1, 1), EndDate = null, Description = null }));

            AssertReachedTheSave("NameHistory.UpdateDto", s, result, "Modified NameHistory");
        }

        [Fact]
        public async Task Web_shape_CompanyDocument_UpdateDto_goes_through()
        {
            var s = new Stack();
            var id = Guid.NewGuid();
            s.Stored(new CompanyDocument { Id = id, CompanyId = Own, CompanyDocumentTypeId = 1, FileName = "a.pdf" });
            var controller = (CompanyDocumentController)s.Controller("CompanyDocument");

            var result = await Call(() => controller.UpdateDto(new CompanyDocumentUpdateDto { Id = id, CompanyDocumentTypeId = 2, FileName = null }));

            AssertReachedTheSave("CompanyDocument.UpdateDto", s, result, "Modified CompanyDocument");
        }

        // ── Routes outside BaseController that carry nested records by design ──────
        // The rule belongs to the inherited write actions only; these routes build their
        // related records on the server and are left as they are.

        [Fact]
        public async Task Company_creation_with_nested_names_is_not_touched_by_the_rule()
        {
            var service = (RecordingService)(object)DispatchProxy.Create(typeof(ICompanyOperationService), typeof(RecordingService));
            var controller = new CompanyOperationController((ICompanyOperationService)(object)service);
            var dto = new CompanyInsertDto
            {
                Translations = { new CompanyTranslationInsertDto { LanguageId = 12, Name = "x" } },
                NameHistory = new CompanyNameHistoryItemDto { StartDate = new DateTime(2020, 1, 1), Translations = { new NameHistoryTranslationInsertDto { LanguageId = 12, Name = "x" } } },
            };

            var result = await Call(() => controller.Insert(dto));

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new[] { "CreateCompanyWithTranslationsAndNameHistoryAsync" }, service.Calls);
            var received = Assert.IsType<CompanyInsertDto>(service.Arguments[0][0]);
            Assert.Single(received.Translations);
            Assert.Single(received.NameHistory!.Translations);
        }

        [Fact]
        public async Task Adding_a_name_with_its_translations_is_not_touched_by_the_rule()
        {
            var service = (RecordingService)(object)DispatchProxy.Create(typeof(ICompanyNameManagementService), typeof(RecordingService));
            var controller = new CompanyNameManagementController((ICompanyNameManagementService)(object)service);
            var dto = new AddNameWithTranslationsDto
            {
                CompanyId = Own,
                StartDate = new DateTime(2021, 1, 1),
                Translations = { new NameHistoryTranslationInsertDto { LanguageId = 12, Name = "x" }, new NameHistoryTranslationInsertDto { LanguageId = 1, Name = "y" } },
            };

            var result = await Call(() => controller.AddNameWithTranslations(dto));

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new[] { "AddNameWithTranslationsAsync" }, service.Calls);
            Assert.Equal(2, Assert.IsType<AddNameWithTranslationsDto>(service.Arguments[0][0]).Translations.Count);
        }

        [Fact]
        public async Task Upserting_name_translations_is_not_touched_by_the_rule()
        {
            var service = (RecordingService)(object)DispatchProxy.Create(typeof(ICompanyNameManagementService), typeof(RecordingService));
            var controller = new CompanyNameManagementController((ICompanyNameManagementService)(object)service);
            var entry = Guid.NewGuid();
            var dto = new UpsertNameTranslationsDto
            {
                NameHistoryId = entry,
                CompanyId = Own,
                Translations = { new NameHistoryTranslationDto { NameHistoryId = entry, LanguageId = 12, Name = "x" } },
            };

            var result = await Call(() => controller.UpsertTranslations(dto));

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(new[] { "UpsertTranslationsAsync" }, service.Calls);
            Assert.Single(Assert.IsType<UpsertNameTranslationsDto>(service.Arguments[0][0]).Translations);
        }

        [Fact]
        public void The_routes_that_carry_nested_records_by_design_are_not_base_controller_routes()
        {
            Assert.DoesNotContain(typeof(CompanyOperationController), BaseControllers());
            Assert.DoesNotContain(typeof(CompanyNameManagementController), BaseControllers());
        }

        // ── Sweeps over every BaseController controller ────────────────────────────

        private static IEnumerable<Type> BaseControllers() => typeof(CompanyController).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.BaseType is { IsGenericType: true } b && b.GetGenericTypeDefinition() == typeof(BaseController<,,,>));

        private static string NameOf(Type controller) => controller.Name.Replace("Controller", string.Empty);

        public static IEnumerable<object[]> EntityWriteActions() =>
            BaseControllers().OrderBy(t => t.Name).SelectMany(t => EntityWrites.Select(a => new object[] { NameOf(t), a }));

        public static IEnumerable<object[]> AllWriteActions() =>
            BaseControllers().OrderBy(t => t.Name).SelectMany(t => AllWrites.Select(a => new object[] { NameOf(t), a }));

        private static (object Controller, RecordingService Service, Type[] Types) Build(string controllerName)
        {
            var type = BaseControllers().Single(t => NameOf(t) == controllerName);
            var contract = type.GetConstructors().Single().GetParameters().Single().ParameterType;
            var proxyType = ScopedControllers.Contains(controllerName) ? typeof(ScopedRecordingService) : typeof(RecordingService);
            var service = DispatchProxy.Create(contract, proxyType);
            return (Activator.CreateInstance(type, service)!, (RecordingService)service, type.BaseType!.GetGenericArguments());
        }

        private static object WithRelatedRecord(Type entity)
        {
            var item = Activator.CreateInstance(entity)!;
            var navigation = entity.GetProperties().First(p => RelatedRecordProbe.IsNavigation(p.PropertyType));
            var target = RelatedRecordProbe.ElementOrSelf(navigation.PropertyType);
            var related = Activator.CreateInstance(target)!;
            if (target == navigation.PropertyType)
            {
                navigation.SetValue(item, related);
            }
            else
            {
                var collection = navigation.GetValue(item) ?? Activator.CreateInstance(typeof(List<>).MakeGenericType(target))!;
                collection.GetType().GetMethod("Add")!.Invoke(collection, new[] { related });
                if (navigation.CanWrite) navigation.SetValue(item, collection);
            }
            return item;
        }

        private static object Argument(string action, Type[] types, object? entity)
        {
            var (t, addDto, updateDto) = (types[0], types[2], types[3]);
            return action switch
            {
                "AddDtoAsync" => Activator.CreateInstance(addDto)!,
                "UpdateDto" => Activator.CreateInstance(updateDto)!,
                "AddRangeAsync" or "UpdateRange" or "RemoveRange" => ToArray(entity ?? Activator.CreateInstance(t)!, t),
                _ => entity ?? Activator.CreateInstance(t)!,
            };
        }

        private static Array ToArray(object item, Type entity)
        {
            var array = Array.CreateInstance(entity, 1);
            array.SetValue(item, 0);
            return array;
        }

        [Theory]
        [MemberData(nameof(EntityWriteActions))]
        public async Task A_write_request_carrying_a_related_record_is_refused_before_the_service(string controller, string action)
        {
            var (instance, service, types) = Build(controller);

            var result = await Call(() => instance.GetType().GetMethod(action)!.Invoke(instance, new[] { Argument(action, types, WithRelatedRecord(types[0])) }));

            Assert.True(IsRefusal(result), $"{controller}.{action} was not refused; the service received: {string.Join(",", service.Calls)}");
            Assert.Empty(service.Calls);
        }

        [Theory]
        [MemberData(nameof(AllWriteActions))]
        public async Task A_flat_write_reaches_only_a_company_scoped_service(string controller, string action)
        {
            var (instance, service, types) = Build(controller);
            var scoped = ScopedControllers.Contains(controller);

            var result = await Call(() => instance.GetType().GetMethod(action)!.Invoke(instance, new[] { Argument(action, types, null) }));

            if (scoped)
            {
                Assert.False(IsRefusal(result), $"{controller}.{action} refused a flat request to a company-scoped service.");
                Assert.Single(service.Calls);
            }
            else
            {
                Assert.True(result is ObjectResult { StatusCode: 403 }, $"{controller}.{action} was not refused; the service received: {string.Join(",", service.Calls)}");
                Assert.Empty(service.Calls);
            }
        }

        // The declaration is read from the instance the controller receives, so the application's
        // own registration must hand each controller its service unwrapped: a decorator or proxy
        // around a declaring service would hide the declaration and refuse every write.
        [Fact]
        public void The_application_registration_gives_each_controller_a_service_whose_declaration_is_visible()
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Type"] = "Negotiate",
                ["ConnectionStrings:DefaultConnection"] = "Server=database.not.available.invalid;Database=none;Encrypt=False",
                ["Services:Person:BaseUrl"] = "http://person.not.available.invalid/",
            }).Build();
            var services = new ServiceCollection();
            services.AddLogging();
            new Startup(configuration).ConfigureServices(services);
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var mismatches = new List<string>();
            var resolved = 0;
            foreach (var controller in BaseControllers())
            {
                var contract = controller.GetConstructors().Single().GetParameters().Single().ParameterType;
                var service = scope.ServiceProvider.GetRequiredService(contract);
                resolved++;
                if (ScopedControllers.Contains(NameOf(controller)) != service is ICompanyScopedWrites)
                    mismatches.Add($"{NameOf(controller)} receives {service.GetType().Name}, which {(service is ICompanyScopedWrites ? "declares" : "does not declare")} company-scoped writes");
            }

            Assert.True(mismatches.Count == 0, string.Join("; ", mismatches));
            Assert.Equal(29, resolved);
        }

        // The DTO write actions are checked for declaration only, not for related records, which
        // holds while the DTOs of the declaring services are flat: no entity, no DTO, and no
        // sequence of either, that a mapping could carry into the saved graph.
        [Fact]
        public void The_dto_writes_of_company_scoped_services_carry_no_related_records()
        {
            var entities = typeof(Company).Assembly;
            var dtos = typeof(FinancialInfoDto).Assembly;
            bool Carries(Type t) => t.IsArray ? Carries(t.GetElementType()!)
                : (t.IsClass && t != typeof(string) && (t.Assembly == entities || t.Assembly == dtos)) || (t.IsGenericType && t.GetGenericArguments().Any(Carries));

            var nested = new List<string>();
            var checkedDtos = 0;
            foreach (var controller in BaseControllers().Where(c => ScopedControllers.Contains(NameOf(c))))
            {
                var arguments = controller.BaseType!.GetGenericArguments();
                foreach (var dto in new[] { arguments[2], arguments[3] })
                {
                    checkedDtos++;
                    nested.AddRange(dto.GetProperties().Where(p => Carries(p.PropertyType)).Select(p => $"{NameOf(controller)} {dto.Name}.{p.Name}"));
                }
            }

            Assert.True(nested.Count == 0, "DTO properties that carry records: " + string.Join(", ", nested));
            Assert.Equal(ScopedControllers.Length * 2, checkedDtos);
        }

        [Fact]
        public void The_services_declaring_company_scoped_writes_are_exactly_the_expected_ones()
        {
            var declared = typeof(CompanyService).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(ICompanyScopedWrites).IsAssignableFrom(t))
                .Select(t => t.Name.Replace("Service", string.Empty))
                .OrderBy(x => x).ToArray();

            Assert.Equal(ScopedControllers.OrderBy(x => x).ToArray(), declared);
        }

        // ── The pin: a declaration is only allowed together with the company checks ────

        public static IEnumerable<object[]> DeclaredServices() => typeof(CompanyService).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ICompanyScopedWrites).IsAssignableFrom(t))
            .OrderBy(t => t.Name).Select(t => new object[] { t.Name });

        [Theory]
        [MemberData(nameof(DeclaredServices))]
        public async Task A_service_declaring_company_scoped_writes_refuses_every_write_without_the_level(string serviceName)
        {
            var type = typeof(CompanyService).Assembly.GetTypes().Single(t => t.Name == serviceName);
            var contract = type.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBaseService<,,,>));
            var (entity, addDto, updateDto) = (contract.GetGenericArguments()[0], contract.GetGenericArguments()[2], contract.GetGenericArguments()[3]);

            var (accessService, access) = FakeAccessService.Create();
            // The caller may change its own company and nothing else.
            access.InformationLevels[Own] = 2;
            var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("personId", Guid.NewGuid().ToString()) }, "test")) } };
            var entry = Guid.NewGuid();
            var fakes = new List<IList>();
            var writes = new List<List<string>>();
            var s = new ServiceCollection();
            s.AddSingleton(accessService);
            s.AddSingleton<IHttpContextAccessor>(accessor);
            s.AddSingleton(CopyMapper.Make());
            foreach (var parameter in type.GetConstructors().Single().GetParameters())
            {
                var repository = parameter.ParameterType.GetInterfaces().Concat(new[] { parameter.ParameterType })
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBaseRepository<>));
                if (repository == null) continue;
                var rowType = repository.GetGenericArguments()[0];
                var (proxy, rows, log) = MakeRepository(rowType, parameter.ParameterType);
                s.AddSingleton(parameter.ParameterType, proxy);
                fakes.Add(rows);
                writes.Add(log);
                if (rowType == typeof(NameHistory) && entity != typeof(NameHistory))
                    rows.Add(new NameHistory { Id = entry, CompanyId = Other, StartDate = new DateTime(2020, 1, 1) });
                if (rowType == entity)
                    rows.Add(Row(entity, entry, Guid.NewGuid()));
            }
            var service = ActivatorUtilities.CreateInstance(s.BuildServiceProvider(), type);
            var stored = fakes.Single(f => f.GetType().GetGenericArguments()[0] == entity)[0]!;

            object Argument(string action, bool claimOwn) => action switch
            {
                "AddAsync" => Row(entity, entry, Guid.NewGuid()),
                "AddRangeAsync" => ToArray(Row(entity, entry, Guid.NewGuid()), entity),
                "AddDtoAsync" => Dto(addDto, stored),
                "UpdateDto" => ClaimOwnCompany(Dto(updateDto, stored), claimOwn),
                "Update" or "Remove" => ClaimOwnCompany(Copy(stored), claimOwn),
                _ => ToArray(ClaimOwnCompany(Copy(stored), claimOwn), entity),
            };

            foreach (var action in AllWrites)
            {
                var method = contract.GetMethods().Single(m => m.Name == action && m.GetParameters().Length == 2);

                // Everything names the other company: the service must check that company's level.
                access.LevelQueries.Clear();
                var result = await Call(() => method.Invoke(service, new[] { Argument(action, false), true }));

                Assert.True(result is BusinessRuleException, $"{serviceName}.{action} did not refuse a caller without the level; outcome: {Outcome(result)}");
                Assert.True(access.LevelQueries.Any(q => q.CompanyId == Other),
                    $"{serviceName}.{action} did not check the company that owns the target or the stored row; checked: {string.Join(",", access.LevelQueries.Select(q => q.CompanyId == Own ? "own" : q.CompanyId.ToString()))}");
                Assert.True(writes.All(w => w.Count == 0), $"{serviceName}.{action} wrote: {string.Join(",", writes.SelectMany(w => w))}");

                // The request claims the caller's own company while its key points at the other
                // company's stored row: only a service that decides by the STORED row refuses it.
                if (action.StartsWith("Add", StringComparison.Ordinal) || !ClaimsCompany(method.GetParameters()[0].ParameterType)) continue;
                result = await Call(() => method.Invoke(service, new[] { Argument(action, true), true }));

                Assert.True(result is BusinessRuleException, $"{serviceName}.{action} accepted a request naming the caller's own company for another company's stored row; outcome: {Outcome(result)}");
                Assert.True(writes.All(w => w.Count == 0), $"{serviceName}.{action} wrote: {string.Join(",", writes.SelectMany(w => w))}");
            }
        }

        // Update and removal requests that carry a company separately from their key. Where the
        // company is part of the key, the request cannot name another company without naming
        // another row.
        private static bool ClaimsCompany(Type parameter)
        {
            var type = parameter.IsGenericType ? parameter.GetGenericArguments()[0] : parameter;
            return type.GetProperty("Id") != null && type.GetProperty("CompanyId") is { PropertyType: var t, CanWrite: true } && t == typeof(Guid);
        }

        private static object ClaimOwnCompany(object request, bool claimOwn)
        {
            if (claimOwn && request.GetType().GetProperty("CompanyId") is { } company)
                company.SetValue(request, Own);
            return request;
        }

        // A row of the other company: CompanyId, or the other company's name history entry.
        private static object Row(Type entity, Guid entry, Guid id)
        {
            var row = Activator.CreateInstance(entity)!;
            entity.GetProperty("CompanyId")?.SetValue(row, Other);
            entity.GetProperty("NameHistoryId")?.SetValue(row, entry);
            entity.GetProperty("LanguageId")?.SetValue(row, (short)12);
            if (entity.GetProperty("Id") is { PropertyType: var idType } idProperty && idType == typeof(Guid)) idProperty.SetValue(row, id);
            if (entity == typeof(NameHistory)) entity.GetProperty("StartDate")!.SetValue(row, new DateTime(2021, 1, 1));
            if (entity == typeof(FinancialInfo)) { entity.GetProperty("FiscalYear")!.SetValue(row, (short)1402); entity.GetProperty("RegisteredCapital")!.SetValue(row, 1m); entity.GetProperty("NumberOfShares")!.SetValue(row, 1L); }
            return row;
        }

        private static object Copy(object row)
        {
            var copy = Activator.CreateInstance(row.GetType())!;
            foreach (var p in row.GetType().GetProperties().Where(p => p.CanRead && p.CanWrite && (p.PropertyType.IsValueType || p.PropertyType == typeof(string))))
                p.SetValue(copy, p.GetValue(row));
            return copy;
        }

        private static object Dto(Type dtoType, object stored)
        {
            var dto = Activator.CreateInstance(dtoType)!;
            foreach (var p in dtoType.GetProperties().Where(p => p.CanWrite))
                if (stored.GetType().GetProperty(p.Name) is { CanRead: true } source && p.PropertyType.IsAssignableFrom(source.PropertyType))
                    p.SetValue(dto, source.GetValue(stored));
            return dto;
        }

        private static (object Proxy, IList Rows, List<string> Writes) MakeRepository(Type rowType, Type repositoryType)
        {
            var closed = typeof(RowRepository<>).MakeGenericType(rowType);
            var keyFunc = typeof(RelatedRecordsTests).GetMethod(nameof(GuidKey), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(rowType).Invoke(null, null)!;
            var made = closed.GetMethod("Make")!.MakeGenericMethod(repositoryType).Invoke(null, new[] { keyFunc })!;
            var proxy = made.GetType().GetField("Item1")!.GetValue(made)!;
            var state = made.GetType().GetField("Item2")!.GetValue(made)!;
            return (proxy, (IList)closed.GetProperty("Rows")!.GetValue(state)!, (List<string>)closed.GetProperty("Writes")!.GetValue(state)!);
        }

        private static Func<T, Guid> GuidKey<T>() where T : class
        {
            var id = typeof(T).GetProperty("Id");
            return row => id != null && id.PropertyType == typeof(Guid) ? (Guid)id.GetValue(row)! : Guid.Empty;
        }

        // ── The detector itself ─────────────────────────────────────────────────────

        [Fact]
        public void Every_base_controller_entity_has_at_least_one_navigation_so_the_sweep_is_meaningful()
        {
            foreach (var controller in BaseControllers())
            {
                var entity = controller.BaseType!.GetGenericArguments()[0];
                Assert.True(entity.GetProperties().Any(p => RelatedRecordProbe.IsNavigation(p.PropertyType)), entity.Name + " has no navigation.");
            }
            Assert.Equal(29, BaseControllers().Count());
        }

        // The detector finds navigations by reflection, while the data layer saves by its own
        // model. A navigation the model knows and the detector misses would be saved without
        // being refused, so the two are compared for every entity a BaseController writes.
        [Fact]
        public void The_detector_finds_exactly_the_navigations_of_the_data_model()
        {
            var model = new Stack().Db.Model;
            var mismatches = new List<string>();
            var compared = 0;
            foreach (var controller in BaseControllers())
            {
                var type = controller.BaseType!.GetGenericArguments()[0];
                var entityType = model.FindEntityType(type);
                if (entityType == null) { mismatches.Add(type.Name + ": not in the data model"); continue; }

                var navigations = entityType.GetNavigations().Select(n => n.Name)
                    .Concat(entityType.GetSkipNavigations().Select(n => n.Name)).ToHashSet();
                var complex = entityType.GetComplexProperties().Select(c => c.Name).ToList();
                if (complex.Count > 0) mismatches.Add($"{type.Name}: complex properties not covered by this comparison: {string.Join(", ", complex)}");

                var detected = RelatedRecords.CarriedBy(FullyPopulated(type)).ToHashSet();
                mismatches.AddRange(navigations.Except(detected).Select(n => $"{type.Name}.{n}: in the data model, not detected"));
                mismatches.AddRange(detected.Except(navigations).Select(n => $"{type.Name}.{n}: detected, not in the data model"));
                if (navigations.Count == 0) mismatches.Add(type.Name + ": the data model reports no navigation, so nothing was compared");
                compared++;
            }

            Assert.True(mismatches.Count == 0, string.Join("; ", mismatches));
            Assert.Equal(29, compared);
        }

        // An instance with every reference-typed property filled and every sequence holding one
        // element, so anything the detector would treat as a related record is present.
        private static object FullyPopulated(Type type)
        {
            var item = Activator.CreateInstance(type)!;
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
            {
                var t = property.PropertyType;
                if (t == typeof(string) || t.IsValueType) continue;
                var element = t.IsArray ? t.GetElementType()
                    : t.GetInterfaces().Concat(t.IsInterface ? new[] { t } : Type.EmptyTypes)
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        .Select(i => i.GetGenericArguments()[0]).FirstOrDefault();
                if (element != null)
                {
                    if (element.GetConstructor(Type.EmptyTypes) == null || element == typeof(string)) continue;
                    var value = property.GetValue(item);
                    var collection = typeof(ICollection<>).MakeGenericType(element);
                    if (value != null && collection.IsInstanceOfType(value) && !t.IsArray)
                    {
                        collection.GetMethod("Add")!.Invoke(value, new[] { Activator.CreateInstance(element) });
                    }
                    else if (property.CanWrite)
                    {
                        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(element))!;
                        list.Add(Activator.CreateInstance(element));
                        if (t.IsArray) { var array = Array.CreateInstance(element, 1); array.SetValue(list[0], 0); property.SetValue(item, array); }
                        else if (t.IsAssignableFrom(list.GetType())) property.SetValue(item, list);
                    }
                }
                else if (property.CanWrite && t.IsClass && t.GetConstructor(Type.EmptyTypes) != null && property.GetValue(item) == null)
                {
                    property.SetValue(item, Activator.CreateInstance(t));
                }
            }
            return item;
        }

        [Fact]
        public void Empty_collections_and_scalars_are_not_related_records()
        {
            Assert.Empty(RelatedRecords.CarriedBy(CompanyRow(Own)));
            Assert.Empty(RelatedRecords.CarriedBy(new FinancialInfoDto { CompanyId = Own }));
            Assert.Empty(RelatedRecords.CarriedBy(null));
        }

        [Fact]
        public void Reference_and_collection_navigations_are_named()
        {
            var graph = CompanyRow(Own);
            graph.Addresses.Add(new Address());
            graph.LegalForm = new LegalForm();
            Assert.Equal(new[] { "Addresses", "LegalForm" }, RelatedRecords.CarriedBy(graph).OrderBy(x => x).ToArray());
        }
    }

    /// <summary>Test-side view of what a navigation is, independent of the code under test.</summary>
    internal static class RelatedRecordProbe
    {
        private static readonly Assembly Entities = typeof(Company).Assembly;

        public static bool IsNavigation(Type type) => IsEntity(type) || (type != typeof(string) && ElementOrSelf(type) is { } e && e != type && IsEntity(e));

        public static Type ElementOrSelf(Type type)
        {
            if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type)) return type.GetGenericArguments()[0];
            return type;
        }

        private static bool IsEntity(Type type) => type.IsClass && type.Assembly == Entities;
    }
}
