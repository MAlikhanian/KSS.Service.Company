using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using AutoMapper;
using KSS.Api.Controller;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;
using KSS.Service.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KSS.Api.Tests
{
    /// <summary>
    /// In-memory repository for one entity type. Reads answer from the rows, writes are
    /// logged, and any other call throws so an unexpected data access fails the test.
    /// </summary>
    public class RowRepository<T> : DispatchProxy where T : class
    {
        public List<T> Rows { get; } = new();
        public List<string> Writes { get; } = new();
        private Func<T, Guid> _key = _ => Guid.Empty;

        public static (TRepository Repository, RowRepository<T> State) Make<TRepository>(Func<T, Guid> key) where TRepository : class
        {
            var proxy = Create<TRepository, RowRepository<T>>();
            var state = (RowRepository<T>)(object)proxy;
            state._key = key;
            return (proxy, state);
        }

        private IQueryable<T> Filter(object? expression) => Rows.AsQueryable().Where((Expression<Func<T, bool>>)expression!);

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            switch (method!.Name)
            {
                case "Find" when args!.Length == 1:
                    return Rows.FirstOrDefault(r => _key(r).Equals(args[0]));
                case "ToList" when args!.Length == 1 && args[0] is LambdaExpression:
                    return Filter(args[0]).ToList();
                case "SingleOrDefault" when args!.Length == 1 && args[0] is LambdaExpression:
                    return Filter(args[0]).SingleOrDefault();
                case "ToListAsync" when args!.Length == 1 && args[0] is LambdaExpression:
                    return Task.FromResult<IEnumerable<T>>(Filter(args[0]).ToList());
                case "AddAsync":
                case "AddRangeAsync":
                    Writes.Add(method.Name);
                    return Task.CompletedTask;
                case "Add":
                case "Update":
                case "UpdateRange":
                case "Remove":
                case "RemoveRange":
                    Writes.Add(method.Name);
                    return null;
            }

            throw new NotSupportedException("Unexpected repository call: " + method.Name);
        }
    }

    /// <summary>Maps the two insert DTOs these services map; anything else throws.</summary>
    public class InsertMapper : DispatchProxy
    {
        public static IMapper Make() => Create<IMapper, InsertMapper>();

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == "Map" && method.IsGenericMethod && args!.Length == 1)
            {
                if (method.ReturnType == typeof(FinancialInfo) && args[0] is FinancialInfoInsertDto f)
                    return new FinancialInfo { CompanyId = f.CompanyId, FiscalYear = f.FiscalYear, RegisteredCapital = f.RegisteredCapital, NumberOfShares = f.NumberOfShares };
                if (method.ReturnType == typeof(NameHistory) && args[0] is NameHistoryInsertDto n)
                    return new NameHistory { CompanyId = n.CompanyId, StartDate = n.StartDate, EndDate = n.EndDate, Description = n.Description };
            }

            throw new NotSupportedException("Unexpected mapper call: " + method.Name);
        }
    }

    /// <summary>
    /// Company-information writes reached through BaseController on the FinancialInfo and
    /// NameHistory controllers. Every write action must check the caller's Information
    /// level 2 on the company it changes: the target for an add, the stored row's company
    /// for an update or a removal. Actions are invoked on the real controllers, as routed.
    /// </summary>
    public class InheritedInformationWriteScopeTests
    {
        private static readonly Guid Own = Guid.Parse("44444444-4444-4444-4444-444444444444");
        private static readonly Guid Other = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private static readonly Guid Caller = Guid.Parse("66666666-6666-6666-6666-666666666666");

        private sealed class Harness
        {
            public FakeAccessService Access { get; }
            public RowRepository<FinancialInfo> FinancialRows { get; }
            public RowRepository<NameHistory> NameRows { get; }
            public IFinancialInfoService FinancialService { get; }
            public INameHistoryService NameService { get; }
            public FinancialInfoController Financial { get; }
            public NameHistoryController Names { get; }

            public Harness(bool withCaller = true)
            {
                var http = new DefaultHttpContext();
                if (withCaller)
                    http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("personId", Caller.ToString()) }, "test"));

                var (accessService, access) = FakeAccessService.Create();
                Access = access;
                var (financialRepository, financialRows) = RowRepository<FinancialInfo>.Make<IFinancialInfoRepository>(r => r.Id);
                var (nameRepository, nameRows) = RowRepository<NameHistory>.Make<INameHistoryRepository>(r => r.Id);
                FinancialRows = financialRows;
                NameRows = nameRows;

                var services = new ServiceCollection();
                services.AddSingleton(accessService);
                services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = http });
                services.AddSingleton(InsertMapper.Make());
                services.AddSingleton(financialRepository);
                services.AddSingleton(nameRepository);
                var provider = services.BuildServiceProvider();

                FinancialService = ActivatorUtilities.CreateInstance<FinancialInfoService>(provider);
                NameService = ActivatorUtilities.CreateInstance<NameHistoryService>(provider);
                Financial = new FinancialInfoController(FinancialService);
                Names = new NameHistoryController(NameService);
            }

            public int WriteCount => FinancialRows.Writes.Count + NameRows.Writes.Count;

            public Guid StoredFinancial(Guid companyId)
            {
                var row = new FinancialInfo { Id = Guid.NewGuid(), CompanyId = companyId, FiscalYear = 1402, RegisteredCapital = 1, NumberOfShares = 1 };
                FinancialRows.Rows.Add(row);
                return row.Id;
            }

            /// <summary>Two entries for the company; returns the newest (current) one.</summary>
            public Guid StoredNameHistory(Guid companyId)
            {
                NameRows.Rows.Add(new NameHistory { Id = Guid.NewGuid(), CompanyId = companyId, StartDate = new DateTime(2020, 1, 1), EndDate = new DateTime(2021, 1, 1) });
                var latest = new NameHistory { Id = Guid.NewGuid(), CompanyId = companyId, StartDate = new DateTime(2021, 1, 1) };
                NameRows.Rows.Add(latest);
                return latest.Id;
            }
        }

        private sealed record WriteCase(string Name, Func<Harness, Guid, Task> Run);

        private static FinancialInfo Financial(Guid id, Guid companyId) =>
            new() { Id = id, CompanyId = companyId, FiscalYear = 1403, RegisteredCapital = 2, NumberOfShares = 2 };

        private static NameHistory Name(Guid id, Guid companyId) =>
            new() { Id = id, CompanyId = companyId, StartDate = new DateTime(2021, 6, 1) };

        private static Task Sync(Action action)
        {
            action();
            return Task.CompletedTask;
        }

        // Every BaseController write action, on both controllers, keyed Controller.Action.
        private static readonly WriteCase[] Cases =
        {
            new("FinancialInfo.AddAsync", (h, c) => h.Financial.AddAsync(Financial(Guid.NewGuid(), c))),
            new("FinancialInfo.AddDtoAsync", (h, c) => h.Financial.AddDtoAsync(new FinancialInfoInsertDto { CompanyId = c, FiscalYear = 1402, RegisteredCapital = 1, NumberOfShares = 1 })),
            new("FinancialInfo.AddRangeAsync", (h, c) => h.Financial.AddRangeAsync(new[] { Financial(Guid.NewGuid(), c), Financial(Guid.NewGuid(), c) })),
            new("FinancialInfo.Update", (h, c) => Sync(() => h.Financial.Update(Financial(h.StoredFinancial(c), c)))),
            new("FinancialInfo.UpdateDto", (h, c) => Sync(() => h.Financial.UpdateDto(new FinancialInfoDto { Id = h.StoredFinancial(c), CompanyId = c, FiscalYear = 1403, RegisteredCapital = 2, NumberOfShares = 2 }))),
            new("FinancialInfo.UpdateRange", (h, c) => Sync(() => h.Financial.UpdateRange(new[] { Financial(h.StoredFinancial(c), c) }))),
            new("FinancialInfo.Remove", (h, c) => Sync(() => h.Financial.Remove(new FinancialInfo { Id = h.StoredFinancial(c), CompanyId = c }))),
            new("FinancialInfo.RemoveRange", (h, c) => Sync(() => h.Financial.RemoveRange(new[] { new FinancialInfo { Id = h.StoredFinancial(c), CompanyId = c } }))),
            new("NameHistory.AddAsync", (h, c) => h.Names.AddAsync(new NameHistory { Id = Guid.NewGuid(), CompanyId = c, StartDate = new DateTime(2025, 1, 1) })),
            new("NameHistory.AddDtoAsync", (h, c) => h.Names.AddDtoAsync(new NameHistoryInsertDto { CompanyId = c, StartDate = new DateTime(2025, 1, 1) })),
            new("NameHistory.AddRangeAsync", (h, c) => h.Names.AddRangeAsync(new[] { new NameHistory { Id = Guid.NewGuid(), CompanyId = c, StartDate = new DateTime(2025, 1, 1) } })),
            new("NameHistory.Update", (h, c) => Sync(() => h.Names.Update(Name(h.StoredNameHistory(c), c)))),
            new("NameHistory.UpdateDto", (h, c) => Sync(() => h.Names.UpdateDto(new NameHistoryDto { Id = h.StoredNameHistory(c), CompanyId = c, StartDate = new DateTime(2021, 6, 1) }))),
            new("NameHistory.UpdateRange", (h, c) => Sync(() => h.Names.UpdateRange(new[] { Name(h.StoredNameHistory(c), c) }))),
            new("NameHistory.Remove", (h, c) => Sync(() => h.Names.Remove(new NameHistory { Id = h.StoredNameHistory(c), CompanyId = c }))),
            new("NameHistory.RemoveRange", (h, c) => Sync(() => h.Names.RemoveRange(new[] { new NameHistory { Id = h.StoredNameHistory(c), CompanyId = c } }))),
        };

        public static IEnumerable<object[]> CaseNames() => Cases.Select(c => new object[] { c.Name });

        private static WriteCase Case(string name) => Cases.Single(c => c.Name == name);

        private static bool IsModifyDenial(Exception? e) =>
            e is BusinessRuleException && e.Message.Contains("do not have permission to modify this company's", StringComparison.Ordinal);

        private static string Outcome(Exception? e) => e == null ? "completed" : e.GetType().Name + ": " + e.Message;

        // ── The sweep: below level 2 on the company, every write action is refused ──

        [Theory]
        [MemberData(nameof(CaseNames))]
        public async Task A_caller_without_level_2_on_the_company_is_refused_before_any_write(string name)
        {
            var h = new Harness();
            h.Access.InformationLevels[Other] = 1;

            var error = await Record.ExceptionAsync(() => Case(name).Run(h, Other));

            Assert.True(IsModifyDenial(error), name + " was not refused; outcome: " + Outcome(error));
            Assert.Equal(0, h.WriteCount);
            Assert.Contains(h.Access.LevelQueries, q => q.CompanyId == Other && q.CallerPersonId == Caller);
        }

        [Theory]
        [MemberData(nameof(CaseNames))]
        public async Task A_caller_with_level_2_on_the_company_reaches_the_write(string name)
        {
            var h = new Harness();
            h.Access.InformationLevels[Own] = 2;

            var error = await Record.ExceptionAsync(() => Case(name).Run(h, Own));

            Assert.True(error == null, name + " failed at level 2; outcome: " + Outcome(error));
            Assert.True(h.WriteCount > 0, name + " wrote nothing at level 2.");
        }

        [Fact]
        public void Every_base_controller_write_action_is_in_the_sweep_for_both_controllers()
        {
            // Reads are POST too, so the verb cannot tell them apart. The application's own
            // permission filter classifies actions; everything it does not map to Read is
            // treated as a write here, so a new unmapped action shows up as uncovered.
            var map = (IReadOnlyDictionary<string, string>)typeof(KSS.Helper.Authorization.PermissionAuthorizationFilter)
                .GetField("ActionToOperation", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null)!;
            var writes = typeof(BaseController<,,,>)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes(inherit: true).OfType<HttpMethodAttribute>().Any())
                .Select(m => m.Name)
                .Where(n => !(map.TryGetValue(n, out var operation) && operation == "Read"))
                .ToHashSet();
            Assert.Contains("FindAsync", typeof(BaseController<,,,>).GetMethods().Select(m => m.Name)); // control: reads exist and are excluded by the map

            foreach (var controller in new[] { "FinancialInfo", "NameHistory" })
            {
                var swept = Cases.Where(c => c.Name.StartsWith(controller + ".", StringComparison.Ordinal))
                    .Select(c => c.Name.Substring(controller.Length + 1))
                    .ToHashSet();
                Assert.True(writes.SetEquals(swept), controller + ": sweep " + string.Join(",", swept.OrderBy(x => x)) + " vs base " + string.Join(",", writes.OrderBy(x => x)));
            }
        }

        // ── Updates and removals: the stored row's company decides ────────────────

        [Fact]
        public async Task A_row_of_another_company_is_refused_even_when_the_request_names_a_permitted_company()
        {
            var h = new Harness();
            h.Access.InformationLevels[Own] = 2;

            var attempts = new (string Name, Func<Task> Run)[]
            {
                ("FinancialInfo.Update", () => Sync(() => h.Financial.Update(Financial(h.StoredFinancial(Other), Own)))),
                ("FinancialInfo.UpdateDto", () => Sync(() => h.Financial.UpdateDto(new FinancialInfoDto { Id = h.StoredFinancial(Other), CompanyId = Own, FiscalYear = 1403, RegisteredCapital = 2, NumberOfShares = 2 }))),
                ("FinancialInfo.Remove", () => Sync(() => h.Financial.Remove(new FinancialInfo { Id = h.StoredFinancial(Other), CompanyId = Own }))),
                ("NameHistory.Update", () => Sync(() => h.Names.Update(Name(h.StoredNameHistory(Other), Own)))),
                ("NameHistory.UpdateDto", () => Sync(() => h.Names.UpdateDto(new NameHistoryDto { Id = h.StoredNameHistory(Other), CompanyId = Own, StartDate = new DateTime(2021, 6, 1) }))),
                ("NameHistory.Remove", () => Sync(() => h.Names.Remove(new NameHistory { Id = h.StoredNameHistory(Other), CompanyId = Own }))),
            };

            foreach (var (name, run) in attempts)
            {
                var error = await Record.ExceptionAsync(run);
                Assert.True(error is BusinessRuleException, name + " was not refused for a row of another company; outcome: " + Outcome(error));
            }

            Assert.Equal(0, h.WriteCount);
            Assert.DoesNotContain(h.Access.LevelQueries, q => q.CompanyId == Own);
        }

        [Fact]
        public async Task A_row_cannot_be_moved_to_another_company()
        {
            var h = new Harness();
            h.Access.InformationLevels[Own] = 2;
            h.Access.InformationLevels[Other] = 2;

            var attempts = new (string Name, Func<Task> Run)[]
            {
                ("FinancialInfo.Update", () => Sync(() => h.Financial.Update(Financial(h.StoredFinancial(Own), Other)))),
                ("FinancialInfo.UpdateDto", () => Sync(() => h.Financial.UpdateDto(new FinancialInfoDto { Id = h.StoredFinancial(Own), CompanyId = Other, FiscalYear = 1403, RegisteredCapital = 2, NumberOfShares = 2 }))),
                ("NameHistory.Update", () => Sync(() => h.Names.Update(Name(h.StoredNameHistory(Own), Other)))),
                ("NameHistory.UpdateDto", () => Sync(() => h.Names.UpdateDto(new NameHistoryDto { Id = h.StoredNameHistory(Own), CompanyId = Other, StartDate = new DateTime(2021, 6, 1) }))),
            };

            foreach (var (name, run) in attempts)
            {
                var error = await Record.ExceptionAsync(run);
                Assert.True(error is BusinessRuleException && error.Message.Contains("cannot be moved to another company", StringComparison.Ordinal),
                    name + " did not reject a move; outcome: " + Outcome(error));
            }

            Assert.Equal(0, h.WriteCount);
        }

        [Theory]
        [InlineData("FinancialInfo.AddDtoAsync")]
        [InlineData("FinancialInfo.Remove")]
        [InlineData("NameHistory.UpdateDto")]
        [InlineData("NameHistory.Remove")]
        public async Task A_request_without_a_caller_is_refused(string name)
        {
            var h = new Harness(withCaller: false);
            h.Access.InformationLevels[Other] = 2;

            var error = await Record.ExceptionAsync(() => Case(name).Run(h, Other));

            Assert.True(error is BusinessRuleException && error.Message.Contains("PersonId", StringComparison.Ordinal),
                name + " did not fail closed; outcome: " + Outcome(error));
            Assert.Equal(0, h.WriteCount);
        }

        // ── Company creation: the unguarded path exists only for the creation service ──

        private const string CreationInterface = "KSS.Service.Service.INewCompanyNameHistory";
        private const string CreationMethod = "AddForNewCompanyAsync";

        [Fact]
        public void The_creation_path_is_internal_and_not_on_any_public_surface()
        {
            var creation = typeof(NameHistoryService).Assembly.GetType(CreationInterface);

            Assert.NotNull(creation);
            Assert.False(creation!.IsPublic || creation.IsNestedPublic);
            Assert.DoesNotContain(typeof(NameHistoryService).GetMethods(BindingFlags.Public | BindingFlags.Instance), m => m.Name.Contains(CreationMethod, StringComparison.Ordinal));
            Assert.DoesNotContain(typeof(INameHistoryService).GetMethods(), m => m.Name.Contains(CreationMethod, StringComparison.Ordinal));
        }

        [Fact]
        public async Task The_creation_path_adds_without_consulting_the_access_service()
        {
            var h = new Harness();
            var creation = typeof(NameHistoryService).Assembly.GetType(CreationInterface)!;
            var method = creation.GetMethod(CreationMethod)!;

            await (Task)method.Invoke(h.NameService, new object[] { new NameHistory { Id = Guid.NewGuid(), CompanyId = Other, StartDate = new DateTime(2025, 1, 1) } })!;

            Assert.Contains("AddAsync", h.NameRows.Writes);
            Assert.Empty(h.Access.LevelQueries);
        }

        // Finds, by IL, every type whose code calls the named method. A token that does not
        // resolve to that exact method is ignored, so operand bytes cannot produce a match.
        private static HashSet<string> CallersOf(string declaringTypeName, string methodName)
        {
            var callers = new HashSet<string>();
            var assemblies = new[] { typeof(NameHistoryService).Assembly, typeof(NameHistoryController).Assembly };
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
            {
                var bodies = type.GetMethods(all).Cast<MethodBase>().Concat(type.GetConstructors(all));
                foreach (var body in bodies)
                {
                    byte[]? il;
                    try { il = body.GetMethodBody()?.GetILAsByteArray(); } catch { continue; }
                    if (il == null) continue;

                    for (var i = 0; i + 4 < il.Length; i++)
                    {
                        if (il[i] != 0x28 && il[i] != 0x6F) continue; // call, callvirt
                        MethodBase? target;
                        try
                        {
                            target = body.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1),
                                type.IsGenericType ? type.GetGenericArguments() : null,
                                body.IsGenericMethod ? body.GetGenericArguments() : null);
                        }
                        catch { continue; }

                        if (target?.Name == methodName && target.DeclaringType?.Name == declaringTypeName)
                        {
                            var top = type;
                            while (top.DeclaringType != null) top = top.DeclaringType;
                            callers.Add(top.Name);
                        }
                    }
                }
            }

            return callers;
        }

        [Fact]
        public void Control_the_call_scanner_finds_a_known_call()
        {
            Assert.Contains("CompanyOperationService", CallersOf("IAccessService", "SeedCreatorAccessAsync"));
        }

        [Fact]
        public void Only_the_company_creation_service_calls_the_creation_path()
        {
            var callers = CallersOf("INewCompanyNameHistory", CreationMethod);

            Assert.Equal(new[] { "CompanyOperationService" }, callers.OrderBy(x => x).ToArray());
        }
    }
}
