using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using KSS.Api.Controller;
using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Helper;
using KSS.Helper.Model;
using KSS.Repository.IRepository;
using KSS.Repository.Repository;
using KSS.Service.IService;
using KSS.Service.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KSS.Api.Tests
{
    /// <summary>The recorder, standing in for a service that declares company-scoped reads.</summary>
    public class ReadScopedRecordingService : RecordingService, ICompanyScopedReads
    {
    }

    /// <summary>The recorder, standing in for a service that declares reference data.</summary>
    public class ReferenceRecordingService : RecordingService, IReferenceData
    {
    }

    /// <summary>
    /// Access service for reads: an Information level per company, or every company (a global
    /// role grant). A caller id of Guid.Empty reads nothing. Any other call throws.
    /// </summary>
    public class ReadAccess : DispatchProxy
    {
        public Dictionary<Guid, int> InformationLevels { get; } = new();
        public bool EveryCompany { get; set; }
        public List<string> Calls { get; } = new();

        public static (IAccessService Service, ReadAccess State) Create()
        {
            var proxy = Create<IAccessService, ReadAccess>();
            return (proxy, (ReadAccess)(object)proxy);
        }

        private int Level(Guid companyId)
        {
            var level = InformationLevels.TryGetValue(companyId, out var value) ? value : 0;
            return EveryCompany ? Math.Max(level, 1) : level;
        }

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            Calls.Add(method!.Name);
            switch (method.Name)
            {
                case "GetLevelsAsync" when args!.Length == 2:
                    return Task.FromResult(new KSS.Dto.AccessLevelsDto { Information = Level((Guid)args[0]!) });
                case "ReadableCompanyIdsAsync" when args!.Length == 1:
                    if ((Guid)args[0]! == Guid.Empty) return Task.FromResult(ReadableCompanies.None);
                    if (EveryCompany) return Task.FromResult(ReadableCompanies.Every);
                    return Task.FromResult(new ReadableCompanies(false, InformationLevels.Where(p => p.Value >= 1).Select(p => p.Key).ToHashSet()));
            }
            throw new NotSupportedException("Unexpected access-service call: " + method.Name);
        }
    }

    /// <summary>
    /// In-memory rows for one entity type, answering the reads the scoped services use: every
    /// row, rows through a predicate, and one row by key. Writes are logged; any other call
    /// throws, so an unexpected data access fails the test.
    /// </summary>
    public class ReadRows<T> : DispatchProxy where T : class
    {
        public List<T> Rows { get; } = new();
        public List<string> Writes { get; } = new();
        private Func<T, object> _key = _ => Guid.Empty;

        public static (TRepository Repository, ReadRows<T> State) Make<TRepository>(Func<T, object> key) where TRepository : class
        {
            var proxy = Create<TRepository, ReadRows<T>>();
            var state = (ReadRows<T>)(object)proxy;
            state._key = key;
            return (proxy, state);
        }

        private IQueryable<T> Filter(object? expression) => Rows.AsQueryable().Where((Expression<Func<T, bool>>)expression!);

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            switch (method!.Name)
            {
                case "ToListAsync" when args!.Length == 0:
                    return Task.FromResult<IEnumerable<T>>(Rows.ToList());
                case "ToListAsync" when args!.Length == 1 && args[0] is LambdaExpression:
                    return Task.FromResult<IEnumerable<T>>(Filter(args[0]).ToList());
                case "FindAsync" when args!.Length == 1 && args[0] is Filter filter:
                    return Task.FromResult(Rows.FirstOrDefault(r => _key(r).Equals(filter.Value)));
                case "Find" when args!.Length == 1:
                    return Rows.FirstOrDefault(r => _key(r).Equals(args[0]));
                case "SingleOrDefault" when args!.Length == 1 && args[0] is LambdaExpression:
                    return Filter(args[0]).SingleOrDefault();
                case "Remove":
                case "RemoveRange":
                case "Update":
                case "UpdateRange":
                    Writes.Add(method.Name);
                    return null;
            }
            throw new NotSupportedException("Unexpected repository call: " + method.Name);
        }
    }

    /// <summary>
    /// The generic read actions of BaseController. A permission is global to the caller, so a
    /// generic read is available only when the service limits it to the companies the caller
    /// may read (ICompanyScopedReads), or when its rows are no company's own records
    /// (IReferenceData). Every other controller refuses its generic reads.
    /// </summary>
    public class ReadScopeTests
    {
        private static readonly Guid Own = Guid.Parse("aaaaaaaa-2222-0000-0000-000000000001");
        private static readonly Guid Other = Guid.Parse("bbbbbbbb-2222-0000-0000-000000000002");
        private static readonly Guid Unreadable = Guid.Parse("cccccccc-2222-0000-0000-000000000003");

        // Listed by hand on purpose: adding or removing a declaration must show up here.
        private static readonly string[] ScopedReadControllers = { "FinancialInfo", "NameHistoryTranslation" };
        private static readonly string[] ReferenceControllers =
        {
            "AddressLabel", "AddressLabelTranslation", "CompanyDocumentType", "CompanyDocumentTypeTranslation",
            "EmailLabel", "EmailLabelTranslation", "Industry", "LegalForm", "PhoneLabel", "PhoneLabelTranslation",
            "Software", "SoftwareCategory", "SoftwareCategoryTranslation", "StakeholderType", "WebsiteLabel", "WebsiteLabelTranslation",
        };
        private static readonly string[] RefusedControllers =
        {
            "Address", "AddressTranslation", "Company", "CompanyDocument", "Email", "NameHistory", "Phone",
            "Stakeholder", "StakeholderHistory", "Translation", "Website",
        };

        private static readonly string[] Reads = { "FindAsync", "SingleAsync", "ToListAllAsync", "ToListAsync", "ToListByFilterAsync", "ToListDtoAsync" };

        private static bool IsRefusal(object? result) => result is ObjectResult { StatusCode: 400 or 403 };

        private static string Outcome(object? result) => result switch
        {
            Exception e => DatabaseSentinel.Reached(e) ? "database reached" : e.GetType().Name + ": " + e.Message,
            ObjectResult o => "status " + o.StatusCode,
            null => "null",
            _ => result.GetType().Name,
        };

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

        private static IHttpContextAccessor Accessor(bool withCaller = true) => new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = withCaller
                    ? new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("personId", Guid.NewGuid().ToString()) }, "test"))
                    : new ClaimsPrincipal(new ClaimsIdentity()),
            },
        };

        private static IEnumerable<Type> BaseControllers() => typeof(CompanyController).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.BaseType is { IsGenericType: true } b && b.GetGenericTypeDefinition() == typeof(BaseController<,,,>));

        private static string NameOf(Type controller) => controller.Name.Replace("Controller", string.Empty);

        private static IEnumerable<Type> ServiceTypes() => typeof(CompanyService).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract);

        private static string[] Declaring<TMarker>() => ServiceTypes()
            .Where(t => typeof(TMarker).IsAssignableFrom(t))
            .Select(t => t.Name.Replace("Service", string.Empty))
            .OrderBy(x => x).ToArray();

        // ── The sweep: every generic read on every BaseController controller ───────────

        public static IEnumerable<object[]> ReadActions() =>
            BaseControllers().OrderBy(t => t.Name).SelectMany(t => Reads.Select(a => new object[] { NameOf(t), a }));

        private static (object Controller, RecordingService Service, Type Entity) Build(string controllerName)
        {
            var type = BaseControllers().Single(t => NameOf(t) == controllerName);
            var contract = type.GetConstructors().Single().GetParameters().Single().ParameterType;
            var proxyType = ScopedReadControllers.Contains(controllerName) ? typeof(ReadScopedRecordingService)
                : ReferenceControllers.Contains(controllerName) ? typeof(ReferenceRecordingService)
                : typeof(RecordingService);
            var service = DispatchProxy.Create(contract, proxyType);
            return (Activator.CreateInstance(type, service)!, (RecordingService)service, type.BaseType!.GetGenericArguments()[0]);
        }

        private static object?[] ReadArguments(string action, Type entity) => action switch
        {
            "ToListAllAsync" => Array.Empty<object?>(),
            "FindAsync" or "ToListByFilterAsync" => new object?[] { new Filter { Value = Guid.NewGuid() } },
            _ => new[] { Activator.CreateInstance(entity) },
        };

        [Theory]
        [MemberData(nameof(ReadActions))]
        public async Task A_generic_read_reaches_only_a_scoped_or_reference_service(string controller, string action)
        {
            var (instance, service, entity) = Build(controller);
            var open = ScopedReadControllers.Contains(controller) || ReferenceControllers.Contains(controller);

            var result = await Call(() => instance.GetType().GetMethod(action)!.Invoke(instance, ReadArguments(action, entity)));

            if (open)
            {
                Assert.False(IsRefusal(result), $"{controller}.{action} refused a read of a scoped or reference service.");
                Assert.NotEmpty(service.Calls);
            }
            else
            {
                Assert.True(result is ObjectResult { StatusCode: 403 }, $"{controller}.{action} was not refused; the service received: {string.Join(",", service.Calls)}");
                Assert.Empty(service.Calls);
            }
        }

        [Fact]
        public void Every_base_controller_read_action_is_in_the_sweep()
        {
            // The application's own permission filter classifies actions; every action it maps
            // to Read must be swept, so a new read action shows up as uncovered.
            var map = (IReadOnlyDictionary<string, string>)typeof(KSS.Helper.Authorization.PermissionAuthorizationFilter)
                .GetField("ActionToOperation", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null)!;
            var reads = typeof(BaseController<,,,>)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes(inherit: true).OfType<HttpMethodAttribute>().Any())
                .Select(m => m.Name)
                .Where(n => map.TryGetValue(n, out var operation) && operation == "Read")
                .ToHashSet();

            Assert.True(reads.SetEquals(Reads), "base " + string.Join(",", reads.OrderBy(x => x)) + " vs sweep " + string.Join(",", Reads.OrderBy(x => x)));
        }

        [Fact]
        public void Every_base_controller_is_in_exactly_one_read_class()
        {
            var controllers = BaseControllers().Select(NameOf).OrderBy(x => x).ToArray();
            var classified = ScopedReadControllers.Concat(ReferenceControllers).Concat(RefusedControllers).OrderBy(x => x).ToArray();

            Assert.Equal(controllers, classified);
            Assert.Equal(29, controllers.Length);
        }

        // ── The declarations ─────────────────────────────────────────────────────────

        [Fact]
        public void The_services_declaring_company_scoped_reads_are_exactly_the_expected_ones()
        {
            Assert.Equal(ScopedReadControllers.OrderBy(x => x).ToArray(), Declaring<ICompanyScopedReads>());
        }

        [Fact]
        public void The_services_declaring_reference_data_are_exactly_the_expected_ones()
        {
            Assert.Equal(ReferenceControllers.OrderBy(x => x).ToArray(), Declaring<IReferenceData>());
        }

        [Fact]
        public void No_service_declares_both_scoped_reads_and_reference_data()
        {
            Assert.Empty(Declaring<ICompanyScopedReads>().Intersect(Declaring<IReferenceData>()));
        }

        // A reference-data row belongs to no company. The one table here that carries a company
        // column is the software catalog, where the column names the product's provider.
        [Fact]
        public void Reference_data_carries_no_company_column_except_the_software_provider()
        {
            var withCompany = ServiceTypes()
                .Where(t => typeof(IReferenceData).IsAssignableFrom(t))
                .Select(t => t.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBaseService<,,,>)).GetGenericArguments()[0])
                .Where(e => e.GetProperty("CompanyId") != null || e.GetProperty("NameHistoryId") != null)
                .Select(e => e.Name)
                .OrderBy(x => x).ToArray();

            Assert.Equal(new[] { "Software" }, withCompany);
        }

        // The declaration is read from the instance the controller receives, so the application's
        // own registration must hand each controller its service unwrapped.
        [Fact]
        public void The_application_registration_gives_each_controller_a_service_whose_read_declaration_is_visible()
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
                var name = NameOf(controller);
                if (ScopedReadControllers.Contains(name) != service is ICompanyScopedReads)
                    mismatches.Add($"{name} receives {service.GetType().Name}, which {(service is ICompanyScopedReads ? "declares" : "does not declare")} company-scoped reads");
                if (ReferenceControllers.Contains(name) != service is IReferenceData)
                    mismatches.Add($"{name} receives {service.GetType().Name}, which {(service is IReferenceData ? "declares" : "does not declare")} reference data");
            }

            Assert.True(mismatches.Count == 0, string.Join("; ", mismatches));
            Assert.Equal(29, resolved);
        }

        // ── The pin: a declaration is only allowed together with the scoping ─────────

        public static IEnumerable<object[]> ScopedReadCases() => ServiceTypes()
            .Where(t => typeof(ICompanyScopedReads).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .SelectMany(t => new[] { "one company", "every company", "no grant", "no caller" }.Select(mode => new object[] { t.Name, mode }));

        private sealed class Pinned
        {
            public object Service = null!;
            public Type Contract = null!;
            public object OwnRow = null!;
            public object OtherRow = null!;
            public Func<object, Guid> CompanyOf = null!;
            public List<List<string>> Writes = new();
        }

        // Builds the declared service on in-memory repositories holding one row of the caller's
        // company and one of another company. The caller may read only its own company.
        private static Pinned Pin(string serviceName, string mode)
        {
            var type = ServiceTypes().Single(t => t.Name == serviceName);
            var contract = type.GetInterfaces().Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBaseService<,,,>));
            var entity = contract.GetGenericArguments()[0];

            var (accessService, access) = ReadAccess.Create();
            if (mode is "one company" or "no caller") access.InformationLevels[Own] = 1;
            if (mode == "every company") access.EveryCompany = true;

            var ownEntry = new NameHistory { Id = Guid.NewGuid(), CompanyId = Own, StartDate = new DateTime(2020, 1, 1) };
            var otherEntry = new NameHistory { Id = Guid.NewGuid(), CompanyId = Other, StartDate = new DateTime(2020, 1, 1) };
            var entries = new Dictionary<Guid, Guid> { [ownEntry.Id] = Own, [otherEntry.Id] = Other };

            var pinned = new Pinned { Contract = contract };
            pinned.CompanyOf = row =>
                entity.GetProperty("CompanyId") is { } company ? (Guid)company.GetValue(row)!
                : entity.GetProperty("NameHistoryId") is { } entry ? entries[(Guid)entry.GetValue(row)!]
                : throw new InvalidOperationException($"Cannot tell which company a {entity.Name} row belongs to; extend this pin.");

            var s = new ServiceCollection();
            s.AddSingleton(accessService);
            s.AddSingleton(Accessor(withCaller: mode != "no caller"));
            s.AddSingleton(CopyMapper.Make());
            foreach (var parameter in type.GetConstructors().Single().GetParameters())
            {
                var repository = parameter.ParameterType.GetInterfaces().Concat(new[] { parameter.ParameterType })
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBaseRepository<>));
                if (repository == null) continue;
                var rowType = repository.GetGenericArguments()[0];
                var (proxy, rows, writes) = MakeRows(rowType, parameter.ParameterType);
                s.AddSingleton(parameter.ParameterType, proxy);
                pinned.Writes.Add(writes);
                if (rowType == typeof(NameHistory) && entity != typeof(NameHistory))
                {
                    rows.Add(ownEntry);
                    rows.Add(otherEntry);
                }
                if (rowType == entity)
                {
                    pinned.OwnRow = Row(entity, Own, ownEntry.Id);
                    pinned.OtherRow = Row(entity, Other, otherEntry.Id);
                    rows.Add(pinned.OwnRow);
                    rows.Add(pinned.OtherRow);
                }
            }

            pinned.Service = ActivatorUtilities.CreateInstance(s.BuildServiceProvider(), type);
            return pinned;
        }

        [Theory]
        [MemberData(nameof(ScopedReadCases))]
        public async Task A_service_declaring_company_scoped_reads_returns_only_readable_companies_rows(string serviceName, string mode)
        {
            var p = Pin(serviceName, mode);
            var list = p.Contract.GetMethods().Single(m => m.Name == "ToListAsync" && m.GetParameters().Length == 0);
            var find = p.Contract.GetMethods().Single(m => m.Name == "FindAsync" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Filter));
            Filter KeyOf(object row) => new() { Value = row.GetType().GetProperty("Id") is { PropertyType: var t } id && t == typeof(Guid) ? id.GetValue(row)! : row.GetType().GetProperty("NameHistoryId")!.GetValue(row)! };

            var listed = await Call(() => list.Invoke(p.Service, null));
            var foundOwn = await Call(() => find.Invoke(p.Service, new object[] { KeyOf(p.OwnRow) }));
            var foundOther = await Call(() => find.Invoke(p.Service, new object[] { KeyOf(p.OtherRow) }));

            switch (mode)
            {
                case "one company":
                    var rows = Assert.IsAssignableFrom<IEnumerable>(listed).Cast<object>().ToList();
                    Assert.True(rows.Count == 1 && rows.All(r => p.CompanyOf(r) == Own),
                        $"{serviceName} listed companies: {string.Join(",", rows.Select(r => p.CompanyOf(r) == Own ? "own" : "OTHER"))}");
                    Assert.Same(p.OwnRow, foundOwn);
                    Assert.True(foundOther is BusinessRuleException, $"{serviceName} returned another company's row; outcome: {Outcome(foundOther)}");
                    break;
                case "every company":
                    Assert.Equal(2, Assert.IsAssignableFrom<IEnumerable>(listed).Cast<object>().Count());
                    Assert.Same(p.OwnRow, foundOwn);
                    Assert.Same(p.OtherRow, foundOther);
                    break;
                case "no grant":
                    Assert.Empty(Assert.IsAssignableFrom<IEnumerable>(listed).Cast<object>());
                    Assert.True(foundOwn is BusinessRuleException, $"{serviceName} returned a row without a grant; outcome: {Outcome(foundOwn)}");
                    Assert.True(foundOther is BusinessRuleException, $"{serviceName} returned a row without a grant; outcome: {Outcome(foundOther)}");
                    break;
                case "no caller":
                    foreach (var outcome in new[] { listed, foundOwn, foundOther })
                        Assert.True(outcome is BusinessRuleException e && e.Message.Contains("PersonId", StringComparison.Ordinal),
                            $"{serviceName} did not fail closed without a caller; outcome: {Outcome(outcome)}");
                    break;
            }

            Assert.True(p.Writes.All(w => w.Count == 0), $"{serviceName} wrote during a read: {string.Join(",", p.Writes.SelectMany(w => w))}");
        }

        // The other four generic reads reach a declared service and end in the base repository,
        // which implements none of them: nothing is returned and the database is not reached.
        // If the repository ever implements one, this fails, and the declared services must
        // scope that read before it ships.
        public static IEnumerable<object[]> ScopedServicesOtherReads() => ScopedReadControllers
            .SelectMany(c => new[] { "SingleAsync", "ToListAsync", "ToListByFilterAsync", "ToListDtoAsync" }.Select(a => new object[] { c, a }));

        [Theory]
        [MemberData(nameof(ScopedServicesOtherReads))]
        public async Task The_other_generic_reads_of_a_scoped_service_return_nothing(string controllerName, string action)
        {
            var sentinel = new DatabaseSentinel();
            var accessor = Accessor();
            using var db = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>()
                .UseSqlServer("Server=database.not.available.invalid;Database=none;Encrypt=False")
                .AddInterceptors(sentinel).Options, accessor);
            var (accessService, _) = ReadAccess.Create();
            var s = new ServiceCollection();
            s.AddSingleton(db);
            s.AddSingleton(accessService);
            s.AddSingleton(accessor);
            s.AddSingleton(CopyMapper.Make());
            foreach (var implementation in typeof(FinancialInfoRepository).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository", StringComparison.Ordinal)))
                foreach (var contract in implementation.GetInterfaces().Where(i => i.Name == "I" + implementation.Name))
                    s.AddSingleton(contract, p => ActivatorUtilities.CreateInstance(p, implementation));
            var provider = s.BuildServiceProvider();

            var type = BaseControllers().Single(t => NameOf(t) == controllerName);
            var serviceContract = type.GetConstructors().Single().GetParameters().Single().ParameterType;
            var implementationType = ServiceTypes().Single(t => serviceContract.IsAssignableFrom(t));
            var controller = Activator.CreateInstance(type, ActivatorUtilities.CreateInstance(provider, implementationType))!;

            var result = await Call(() => type.GetMethod(action)!.Invoke(controller, ReadArguments(action, type.BaseType!.GetGenericArguments()[0])));

            Assert.True(result is NotImplementedException, $"{controllerName}.{action} returned something; outcome: {Outcome(result)}");
            Assert.Equal(0, sentinel.Attempts);
        }

        // ── The routes the web applications use, in the shapes they send ──────────────

        private sealed class FinancialRoute
        {
            public ReadAccess Access { get; }
            public ReadRows<FinancialInfo> Rows { get; }
            public FinancialInfoController Controller { get; }

            public FinancialRoute()
            {
                var (accessService, access) = ReadAccess.Create();
                Access = access;
                var (repository, rows) = ReadRows<FinancialInfo>.Make<IFinancialInfoRepository>(r => r.Id);
                Rows = rows;
                var s = new ServiceCollection();
                s.AddSingleton(accessService);
                s.AddSingleton(Accessor());
                s.AddSingleton(CopyMapper.Make());
                s.AddSingleton(repository);
                Controller = new FinancialInfoController(ActivatorUtilities.CreateInstance<FinancialInfoService>(s.BuildServiceProvider()));
            }

            public FinancialInfo Stored(Guid companyId, short year)
            {
                var row = new FinancialInfo { Id = Guid.NewGuid(), CompanyId = companyId, FiscalYear = year, RegisteredCapital = 1000, NumberOfShares = 10 };
                Rows.Rows.Add(row);
                return row;
            }

            // The web application's list: every row the route returns, filtered by the company id
            // in its own URL.
            public async Task<List<FinancialInfo>> ListFor(Guid companyId)
            {
                var result = await Call(() => Controller.ToListAllAsync());
                var rows = Assert.IsAssignableFrom<IEnumerable<FinancialInfo>>(Assert.IsType<OkObjectResult>(result).Value);
                return rows.Where(r => r.CompanyId == companyId).ToList();
            }
        }

        // The web application removes a row by listing, finding the row by id among the
        // company's rows, and sending it back with five fields. Reads must not break it.
        [Fact]
        public async Task Web_shape_FinancialInfo_delete_lists_finds_and_removes_the_companys_row()
        {
            var route = new FinancialRoute();
            route.Access.InformationLevels[Own] = 2;
            var target = route.Stored(Own, 1402);
            route.Stored(Own, 1403);
            route.Stored(Other, 1402);

            var record = (await route.ListFor(Own)).Single(r => r.Id == target.Id);
            var removed = await Call(() => route.Controller.Remove(new FinancialInfo
            {
                Id = record.Id, CompanyId = record.CompanyId, FiscalYear = record.FiscalYear,
                RegisteredCapital = record.RegisteredCapital, NumberOfShares = record.NumberOfShares,
            }));

            Assert.True(removed is NoContentResult, "FinancialInfo delete did not go through; outcome: " + Outcome(removed));
            Assert.Equal(new[] { "Remove" }, route.Rows.Writes);
        }

        [Fact]
        public async Task Web_shape_FinancialInfo_list_for_another_companys_id_returns_nothing()
        {
            var route = new FinancialRoute();
            route.Access.InformationLevels[Own] = 1;
            route.Stored(Own, 1402);
            route.Stored(Other, 1402);
            route.Stored(Other, 1403);

            Assert.Single(await route.ListFor(Own));
            Assert.Empty(await route.ListFor(Other));
        }

        [Fact]
        public async Task Web_shape_FinancialInfo_delete_for_another_companys_id_finds_no_row()
        {
            var route = new FinancialRoute();
            route.Access.InformationLevels[Own] = 2;
            var target = route.Stored(Other, 1402);

            Assert.DoesNotContain(await route.ListFor(Other), r => r.Id == target.Id);
            Assert.Empty(route.Rows.Writes);
        }

        [Fact]
        public async Task Web_shape_NameHistoryTranslation_list_returns_only_readable_companies_translations()
        {
            var (accessService, access) = ReadAccess.Create();
            access.InformationLevels[Own] = 1;
            var (translations, translationRows) = ReadRows<NameHistoryTranslation>.Make<INameHistoryTranslationRepository>(r => r.NameHistoryId);
            var (histories, historyRows) = ReadRows<NameHistory>.Make<INameHistoryRepository>(r => r.Id);
            var ownEntry = new NameHistory { Id = Guid.NewGuid(), CompanyId = Own, StartDate = new DateTime(2020, 1, 1) };
            var otherEntry = new NameHistory { Id = Guid.NewGuid(), CompanyId = Other, StartDate = new DateTime(2020, 1, 1) };
            historyRows.Rows.AddRange(new[] { ownEntry, otherEntry });
            translationRows.Rows.Add(new NameHistoryTranslation { NameHistoryId = ownEntry.Id, LanguageId = 12, Name = "own" });
            translationRows.Rows.Add(new NameHistoryTranslation { NameHistoryId = ownEntry.Id, LanguageId = 10, Name = "own" });
            translationRows.Rows.Add(new NameHistoryTranslation { NameHistoryId = otherEntry.Id, LanguageId = 12, Name = "other" });
            var s = new ServiceCollection();
            s.AddSingleton(accessService);
            s.AddSingleton(Accessor());
            s.AddSingleton(CopyMapper.Make());
            s.AddSingleton(translations);
            s.AddSingleton(histories);
            var controller = new NameHistoryTranslationController(ActivatorUtilities.CreateInstance<NameHistoryTranslationService>(s.BuildServiceProvider()));

            var result = await Call(() => controller.ToListAllAsync());

            var rows = Assert.IsAssignableFrom<IEnumerable<NameHistoryTranslation>>(Assert.IsType<OkObjectResult>(result).Value).ToList();
            Assert.Equal(2, rows.Count);
            Assert.All(rows, r => Assert.Equal(ownEntry.Id, r.NameHistoryId));
        }

        // ── The SQL the real repositories send for a list ─────────────────────────────

        private static (MainDbContext Db, CommandCapture Capture, IHttpContextAccessor Accessor) CapturingContext()
        {
            var capture = new CommandCapture();
            var accessor = Accessor();
            var db = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>()
                .UseSqlServer("Server=database.not.available.invalid;Database=none;Encrypt=False")
                .AddInterceptors(new NoOpenConnection(), capture).Options, accessor);
            return (db, capture, accessor);
        }

        private static IAccessService Readable(params Guid[] companies)
        {
            var readable = DispatchProxy.Create<IAccessService, FixedReadable>();
            ((FixedReadable)(object)readable).Result = new ReadableCompanies(false, companies.ToHashSet());
            return readable;
        }

        [Fact]
        public async Task The_FinancialInfo_list_filters_by_company_in_sql()
        {
            var (db, capture, accessor) = CapturingContext();
            using (db)
            {
                var service = new FinancialInfoService(FakeMapper.Create(), new FinancialInfoRepository(db), Readable(Own, Other), accessor);

                try { await service.ToListAsync(); } catch (InvalidOperationException) { }

                var sql = capture.Commands.FirstOrDefault() ?? "(no command sent)";
                Assert.True(sql.Contains("FROM [dbo].[FinancialInfo]") && sql.Contains("WHERE") && sql.Contains(Own.ToString()) && sql.Contains(Other.ToString()) && !sql.Contains(Unreadable.ToString()),
                    "The FinancialInfo list is not filtered by the readable companies in SQL:\n" + sql);
            }
        }

        [Fact]
        public async Task The_NameHistoryTranslation_list_resolves_entries_of_the_readable_companies_in_sql()
        {
            var (db, capture, accessor) = CapturingContext();
            using (db)
            {
                var service = new NameHistoryTranslationService(FakeMapper.Create(), new NameHistoryTranslationRepository(db), new NameHistoryRepository(db), Readable(Own, Other), accessor);

                try { await service.ToListAsync(); } catch (InvalidOperationException) { }

                var sql = capture.Commands.FirstOrDefault() ?? "(no command sent)";
                Assert.True(sql.Contains("FROM [dbo].[NameHistory]") && sql.Contains("WHERE") && sql.Contains(Own.ToString()) && sql.Contains(Other.ToString()),
                    "The name history entries are not filtered by the readable companies in SQL:\n" + sql);
                Assert.DoesNotContain(capture.Commands, c => c.Contains("[dbo].[NameHistoryTranslation]") && !c.Contains("WHERE"));
            }
        }

        // ── Helpers ─────────────────────────────────────────────────────────────────

        private static object Row(Type entity, Guid companyId, Guid entry)
        {
            var row = Activator.CreateInstance(entity)!;
            entity.GetProperty("CompanyId")?.SetValue(row, companyId);
            entity.GetProperty("NameHistoryId")?.SetValue(row, entry);
            entity.GetProperty("LanguageId")?.SetValue(row, (short)12);
            if (entity.GetProperty("Id") is { PropertyType: var idType } idProperty && idType == typeof(Guid)) idProperty.SetValue(row, Guid.NewGuid());
            return row;
        }

        private static (object Proxy, IList Rows, List<string> Writes) MakeRows(Type rowType, Type repositoryType)
        {
            var closed = typeof(ReadRows<>).MakeGenericType(rowType);
            var keyFunc = typeof(ReadScopeTests).GetMethod(nameof(KeyOf), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(rowType).Invoke(null, null)!;
            var made = closed.GetMethod("Make")!.MakeGenericMethod(repositoryType).Invoke(null, new[] { keyFunc })!;
            var proxy = made.GetType().GetField("Item1")!.GetValue(made)!;
            var state = made.GetType().GetField("Item2")!.GetValue(made)!;
            return (proxy, (IList)closed.GetProperty("Rows")!.GetValue(state)!, (List<string>)closed.GetProperty("Writes")!.GetValue(state)!);
        }

        // The key a find by one value resolves: the Guid Id, or for a translation keyed by its
        // name history entry and language, the entry.
        private static Func<T, object> KeyOf<T>() where T : class
        {
            var id = typeof(T).GetProperty("Id");
            var entry = typeof(T).GetProperty("NameHistoryId");
            return row => id != null && id.PropertyType == typeof(Guid) ? id.GetValue(row)! : entry?.GetValue(row) ?? Guid.Empty;
        }
    }
}
