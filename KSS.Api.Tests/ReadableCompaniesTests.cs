using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using KSS.Data.DbContexts;
using KSS.Entity;
using KSS.Repository.IRepository;
using KSS.Repository.Repository;
using KSS.Service.IService;
using KSS.Service.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KSS.Api.Tests
{
    /// <summary>
    /// In-memory grant rows. A read answers through the predicate the service passes; a read
    /// without a predicate returns every row, so an unfiltered read shows up in the result.
    /// Writes are logged; anything else throws.
    /// </summary>
    public class GrantRepository<T> : DispatchProxy where T : class
    {
        public List<T> Rows { get; } = new();
        public List<(string Operation, T? Row)> Writes { get; } = new();

        public static (TRepository Repository, GrantRepository<T> State) Make<TRepository>() where TRepository : class
        {
            var proxy = Create<TRepository, GrantRepository<T>>();
            return (proxy, (GrantRepository<T>)(object)proxy);
        }

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            switch (method!.Name)
            {
                case "ToListAsync" when args!.Length == 0:
                    return Task.FromResult<IEnumerable<T>>(Rows.ToList());
                case "ToListAsync" when args!.Length == 1 && args[0] is LambdaExpression:
                    return Task.FromResult<IEnumerable<T>>(Rows.AsQueryable().Where((Expression<Func<T, bool>>)args[0]!).ToList());
                case "Remove":
                    Writes.Add(("Remove", (T?)args![0]));
                    return null;
                case "AddUnawaited":
                    Writes.Add(("Add", (T?)args![0]));
                    return null;
                case "SaveChangesAsync":
                    Writes.Add(("Save", null));
                    return Task.CompletedTask;
            }
            throw new NotSupportedException("Unexpected repository call: " + method.Name);
        }
    }

    /// <summary>An access service that answers only the read rule, with a fixed result.</summary>
    public class FixedReadable : DispatchProxy
    {
        public ReadableCompanies Result { get; set; } = ReadableCompanies.None;
        public int Calls { get; private set; }

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == "ReadableCompanyIdsAsync") { Calls++; return Task.FromResult(Result); }
            throw new NotSupportedException("Unexpected access-service call: " + method.Name);
        }
    }

    /// <summary>Refuses to open any connection; the command text is captured and refused before it runs.</summary>
    public sealed class NoOpenConnection : DbConnectionInterceptor
    {
        public override InterceptionResult ConnectionOpening(DbConnection c, ConnectionEventData d, InterceptionResult r) => InterceptionResult.Suppress();
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection c, ConnectionEventData d, InterceptionResult r, CancellationToken t = default) => ValueTask.FromResult(InterceptionResult.Suppress());
    }

    public sealed class CommandCapture : DbCommandInterceptor
    {
        public List<string> Commands { get; } = new();

        private InterceptionResult<DbDataReader> Refuse(DbCommand command)
        {
            var text = new StringBuilder();
            foreach (DbParameter p in command.Parameters) text.AppendLine($"-- {p.ParameterName} = {p.Value}");
            Commands.Add(text + command.CommandText);
            throw new InvalidOperationException("Command captured and refused before execution.");
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand c, CommandEventData d, InterceptionResult<DbDataReader> r) => Refuse(c);
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand c, CommandEventData d, InterceptionResult<DbDataReader> r, CancellationToken t = default) => ValueTask.FromResult(Refuse(c));
        public override InterceptionResult<object> ScalarExecuting(DbCommand c, CommandEventData d, InterceptionResult<object> r) { Refuse(c); return r; }
        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand c, CommandEventData d, InterceptionResult<object> r, CancellationToken t = default) { Refuse(c); return ValueTask.FromResult(r); }
    }

    /// <summary>
    /// The read rule for company information: live grants only, Information level 1 or more,
    /// personal or through a role, a global role grant at that level covering every company.
    /// The grant writes still see inactive and deleted rows, because the unique indexes on
    /// Access and RoleAccess do not include IsActive.
    /// </summary>
    public class ReadableCompaniesTests
    {
        private static readonly Guid Caller = Guid.Parse("11111111-aaaa-0000-0000-000000000001");
        private static readonly Guid Other = Guid.Parse("22222222-aaaa-0000-0000-000000000002");
        private static readonly Guid Third = Guid.Parse("33333333-aaaa-0000-0000-000000000003");
        private static readonly Guid CompanyA = Guid.Parse("aaaaaaaa-0000-0000-0000-00000000000a");
        private static readonly Guid CompanyB = Guid.Parse("bbbbbbbb-0000-0000-0000-00000000000b");
        private static readonly Guid CompanyC = Guid.Parse("cccccccc-0000-0000-0000-00000000000c");
        private static readonly Guid CompanyD = Guid.Parse("dddddddd-0000-0000-0000-00000000000d");
        private static readonly Guid RoleX = Guid.Parse("99999999-0000-0000-0000-000000000001");
        private static readonly Guid RoleY = Guid.Parse("99999999-0000-0000-0000-000000000002");
        private static readonly DateTime Deleted = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static IHttpContextAccessor AccessorFor(Guid person, params Guid[] roles) => new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim("personId", person.ToString()) }.Concat(roles.Select(r => new Claim("roleId", r.ToString()))), "test")),
            },
        };

        private static Access Grant(Guid company, Guid person, byte section, int level, bool active = true, DateTime? deleted = null) =>
            new() { Id = Guid.NewGuid(), CompanyId = company, GrantedToPersonId = person, SectionId = section, Level = level, IsActive = active, DeletedAt = deleted };

        private static RoleAccess RoleGrant(Guid? company, Guid role, byte section, int level, bool active = true, DateTime? deleted = null) =>
            new() { Id = Guid.NewGuid(), CompanyId = company, GrantedToRoleId = role, SectionId = section, Level = level, IsActive = active, DeletedAt = deleted };

        private sealed record Setup(AccessService Access, GrantRepository<Access> Grants, GrantRepository<RoleAccess> RoleGrants, IHttpContextAccessor Accessor);

        private static Setup Build(Guid person, params Guid[] roles)
        {
            var (accessRepository, grants) = GrantRepository<Access>.Make<IAccessRepository>();
            var (roleRepository, roleGrants) = GrantRepository<RoleAccess>.Make<IRoleAccessRepository>();
            var accessor = AccessorFor(person, roles);
            return new Setup(new AccessService(FakeMapper.Create(), accessRepository, roleRepository, accessor), grants, roleGrants, accessor);
        }

        // Called by name, so these tests also compile and fail by name where the rule is missing.
        private static async Task<ReadableCompanies> Readable(AccessService service, Guid person)
        {
            var method = typeof(AccessService).GetMethod("ReadableCompanyIdsAsync");
            Assert.True(method != null, "AccessService has no ReadableCompanyIdsAsync: the shared read rule is missing.");
            var task = (Task)method!.Invoke(service, new object[] { person })!;
            await task;
            return (ReadableCompanies)task.GetType().GetProperty("Result")!.GetValue(task)!;
        }

        // ── Live grants only, on every read of grants ───────────────────────────────

        [Fact]
        public async Task Levels_ignore_inactive_and_deleted_personal_grants()
        {
            var s = Build(Caller);
            s.Grants.Rows.Add(Grant(CompanyA, Caller, AccessSectionId.Information, 2, active: false));
            s.Grants.Rows.Add(Grant(CompanyA, Caller, AccessSectionId.Access, 2, deleted: Deleted));
            s.Grants.Rows.Add(Grant(CompanyB, Caller, AccessSectionId.Information, 1));

            var a = await s.Access.GetLevelsAsync(CompanyA, Caller);
            var b = await s.Access.GetLevelsAsync(CompanyB, Caller);

            Assert.True(a.Information == 0 && a.Access == 0, $"An inactive or deleted grant still counts: Information {a.Information}, Access {a.Access} on company A.");
            Assert.Equal(1, b.Information);
        }

        [Fact]
        public async Task Levels_ignore_inactive_and_deleted_role_grants()
        {
            var s = Build(Caller, RoleX);
            s.RoleGrants.Rows.Add(RoleGrant(null, RoleX, AccessSectionId.Information, 2, active: false));
            s.RoleGrants.Rows.Add(RoleGrant(CompanyA, RoleX, AccessSectionId.Access, 2, deleted: Deleted));

            var a = await s.Access.GetLevelsAsync(CompanyA, Caller);

            Assert.True(a.Information == 0 && a.Access == 0, $"An inactive or deleted role grant still counts: Information {a.Information}, Access {a.Access}.");
        }

        [Fact]
        public async Task Grant_lists_leave_out_inactive_and_deleted_grants()
        {
            var s = Build(Caller);
            s.Grants.Rows.Add(Grant(CompanyA, Other, AccessSectionId.Information, 2));
            s.Grants.Rows.Add(Grant(CompanyA, Third, AccessSectionId.Information, 1, active: false));
            s.Grants.Rows.Add(Grant(CompanyB, Third, AccessSectionId.Information, 1, deleted: Deleted));

            var byCompany = await s.Access.ListGrantsByCompanyAsync(CompanyA);
            var pairs = await s.Access.ListAllGrantPairsAsync();

            Assert.Equal(new[] { Other }, byCompany.Select(g => g.GrantedToPersonId).ToArray());
            Assert.True(pairs.Count == 1 && pairs[0].GrantedToPersonId == Other && pairs[0].CompanyId == CompanyA,
                "ListAllGrantPairs includes inactive or deleted grants: " + string.Join(", ", pairs.Select(p => $"{p.CompanyId}/{p.GrantedToPersonId}")));
        }

        [Fact]
        public async Task Role_grant_lists_leave_out_inactive_and_deleted_grants()
        {
            var s = Build(Caller);
            s.RoleGrants.Rows.Add(RoleGrant(CompanyA, RoleX, AccessSectionId.Information, 2));
            s.RoleGrants.Rows.Add(RoleGrant(CompanyA, RoleY, AccessSectionId.Information, 1, active: false));
            s.RoleGrants.Rows.Add(RoleGrant(null, RoleY, AccessSectionId.Information, 1, deleted: Deleted));
            var service = new RoleAccessService(FakeMapper.Create(), (IRoleAccessRepository)(object)s.RoleGrants, s.Access);

            var byCompany = await service.ListGrantsByCompanyAsync(CompanyA);
            var all = await service.ListAllGrantsAsync();

            Assert.True(byCompany.Count == 1 && byCompany[0].GrantedToRoleId == RoleX,
                "Role grants by company include inactive or deleted grants: " + string.Join(", ", byCompany.Select(g => g.GrantedToRoleId)));
            Assert.True(all.Count == 1 && all[0].GrantedToRoleId == RoleX,
                "All role grants include inactive or deleted grants: " + string.Join(", ", all.Select(g => g.GrantedToRoleId)));
        }

        // ── The grant writes still see every row (control: unchanged behaviour) ──────

        [Fact]
        public async Task Grant_writes_still_replace_and_revoke_inactive_and_deleted_rows()
        {
            var s = Build(Caller, RoleX);
            s.Grants.Rows.Add(Grant(CompanyA, Caller, AccessSectionId.Access, 2));
            var deadPersonal = Grant(CompanyA, Other, AccessSectionId.Information, 1, active: false);
            var deletedPersonal = Grant(CompanyA, Third, AccessSectionId.Information, 1, deleted: Deleted);
            s.Grants.Rows.AddRange(new[] { deadPersonal, deletedPersonal });
            var deadRole = RoleGrant(CompanyA, RoleY, AccessSectionId.Information, 1, active: false);
            s.RoleGrants.Rows.Add(deadRole);
            var roleService = new RoleAccessService(FakeMapper.Create(), (IRoleAccessRepository)(object)s.RoleGrants, s.Access);

            await s.Access.UpsertGrantAsync(new KSS.Dto.AccessGrantDto { CompanyId = CompanyA, GrantedToPersonId = Other, InformationLevel = 2 }, Caller);
            await s.Access.RevokeByPairAsync(CompanyA, Third, Caller);
            await roleService.UpsertGrantAsync(new KSS.Dto.RoleAccessGrantDto { CompanyId = CompanyA, GrantedToRoleId = RoleY, InformationLevel = 2 }, Caller);

            Assert.Contains(s.Grants.Writes, w => w.Operation == "Remove" && ReferenceEquals(w.Row, deadPersonal));
            Assert.Contains(s.Grants.Writes, w => w.Operation == "Remove" && ReferenceEquals(w.Row, deletedPersonal));
            Assert.Contains(s.RoleGrants.Writes, w => w.Operation == "Remove" && ReferenceEquals(w.Row, deadRole));
        }

        // ── The read rule ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Readable_counts_live_information_grants_of_level_one_or_more()
        {
            var s = Build(Caller, RoleX);
            s.Grants.Rows.Add(Grant(CompanyA, Caller, AccessSectionId.Information, 1));
            s.Grants.Rows.Add(Grant(CompanyB, Caller, AccessSectionId.Access, 2));
            s.Grants.Rows.Add(Grant(CompanyC, Caller, AccessSectionId.Information, 2, active: false));
            s.Grants.Rows.Add(Grant(CompanyD, Caller, AccessSectionId.Information, 2, deleted: Deleted));
            s.Grants.Rows.Add(Grant(CompanyB, Other, AccessSectionId.Information, 2));
            s.RoleGrants.Rows.Add(RoleGrant(CompanyC, RoleX, AccessSectionId.Information, 1));
            s.RoleGrants.Rows.Add(RoleGrant(CompanyD, RoleX, AccessSectionId.Access, 2));
            s.RoleGrants.Rows.Add(RoleGrant(CompanyB, RoleY, AccessSectionId.Information, 2));

            var readable = await Readable(s.Access, Caller);

            Assert.False(readable.All, "An explicit set was expected, not every company.");
            Assert.Equal(new[] { CompanyA, CompanyC }.OrderBy(x => x), readable.CompanyIds.OrderBy(x => x));
        }

        [Fact]
        public async Task Readable_is_every_company_only_for_a_live_global_information_grant()
        {
            var information = Build(Caller, RoleX);
            information.RoleGrants.Rows.Add(RoleGrant(null, RoleX, AccessSectionId.Information, 1));

            var accessOnly = Build(Caller, RoleX);
            accessOnly.RoleGrants.Rows.Add(RoleGrant(null, RoleX, AccessSectionId.Access, 2));
            accessOnly.Grants.Rows.Add(Grant(CompanyA, Caller, AccessSectionId.Information, 1));

            var inactive = Build(Caller, RoleX);
            inactive.RoleGrants.Rows.Add(RoleGrant(null, RoleX, AccessSectionId.Information, 2, active: false));

            Assert.True((await Readable(information.Access, Caller)).All, "A live global Information grant should cover every company.");
            var viaAccess = await Readable(accessOnly.Access, Caller);
            Assert.False(viaAccess.All, "A global grant on the Access section alone covered every company.");
            Assert.Equal(new[] { CompanyA }, viaAccess.CompanyIds.ToArray());
            Assert.False((await Readable(inactive.Access, Caller)).All, "An inactive global grant covered every company.");
        }

        [Fact]
        public async Task Readable_is_nothing_without_a_caller()
        {
            var s = Build(Caller, RoleX);
            s.RoleGrants.Rows.Add(RoleGrant(null, RoleX, AccessSectionId.Information, 2));

            var readable = await Readable(s.Access, Guid.Empty);

            Assert.False(readable.All);
            Assert.Empty(readable.CompanyIds);
        }

        // Two computations of the same rule, compared company by company.
        [Fact]
        public async Task Readable_agrees_with_levels_for_every_company()
        {
            var s = Build(Caller, RoleX, RoleY);
            s.Grants.Rows.Add(Grant(CompanyA, Caller, AccessSectionId.Information, 2));
            s.Grants.Rows.Add(Grant(CompanyB, Caller, AccessSectionId.Access, 2));
            s.Grants.Rows.Add(Grant(CompanyC, Caller, AccessSectionId.Information, 1, active: false));
            s.RoleGrants.Rows.Add(RoleGrant(CompanyC, RoleY, AccessSectionId.Information, 1));
            s.RoleGrants.Rows.Add(RoleGrant(CompanyD, RoleX, AccessSectionId.Information, 1, deleted: Deleted));

            var readable = await Readable(s.Access, Caller);
            var disagreements = new List<string>();
            foreach (var company in new[] { CompanyA, CompanyB, CompanyC, CompanyD })
            {
                var levels = await s.Access.GetLevelsAsync(company, Caller);
                if (readable.Includes(company) != (levels.Information >= 1))
                    disagreements.Add($"{company}: readable {readable.Includes(company)}, Information level {levels.Information}");
            }

            Assert.True(disagreements.Count == 0, string.Join("; ", disagreements));
        }

        // ── The SQL the real repositories send ─────────────────────────────────────

        [Fact]
        public async Task The_level_queries_filter_live_rows_in_sql()
        {
            var capture = new CommandCapture();
            var accessor = AccessorFor(Caller, RoleX);
            using var db = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>()
                .UseSqlServer("Server=database.not.available.invalid;Database=none;Encrypt=False")
                .AddInterceptors(new NoOpenConnection(), capture).Options, accessor);
            var service = new AccessService(FakeMapper.Create(), new AccessRepository(db), new RoleAccessRepository(db), accessor);

            try { await service.GetLevelsAsync(CompanyA, Caller); } catch (InvalidOperationException) { }

            var sql = capture.Commands.FirstOrDefault() ?? "(no command sent)";
            Assert.True(sql.Contains("[IsActive] = CAST(1 AS bit)") && sql.Contains("[DeletedAt] IS NULL"),
                "The Access query of GetLevelsAsync does not filter live rows:\n" + sql);
        }

        // ── The company list uses the same rule ────────────────────────────────────

        private static (object Service, CommandCapture Capture, FixedReadable Readable) SelectService(ReadableCompanies result)
        {
            var capture = new CommandCapture();
            var accessor = AccessorFor(Caller, RoleX);
            var db = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>()
                .UseSqlServer("Server=database.not.available.invalid;Database=none;Encrypt=False")
                .AddInterceptors(new NoOpenConnection(), capture).Options, accessor);
            var readable = DispatchProxy.Create<IAccessService, FixedReadable>();
            ((FixedReadable)(object)readable).Result = result;
            var services = new ServiceCollection();
            services.AddSingleton(db);
            services.AddSingleton(readable);
            services.AddSingleton<IHttpContextAccessor>(accessor);
            var provider = services.BuildServiceProvider();
            return (ActivatorUtilities.CreateInstance<CompanySelectService>(provider), capture, (FixedReadable)(object)readable);
        }

        private static async Task<object?> CallSelect(object service)
        {
            try { return await ((CompanySelectService)service).GetCompanySelectListAsync(12); }
            catch (Exception e) { return e; }
        }

        [Fact]
        public async Task The_company_list_is_limited_to_the_readable_companies()
        {
            var (service, capture, readable) = SelectService(new ReadableCompanies(false, new HashSet<Guid> { CompanyA, CompanyB }));

            await CallSelect(service);

            Assert.True(readable.Calls == 1, "The company list did not ask for the readable companies; it computed visibility itself.");
            var first = capture.Commands.FirstOrDefault() ?? "(no command sent)";
            Assert.True(first.Contains("FROM [dbo].[Company]") && first.Contains(CompanyA.ToString()) && first.Contains(CompanyB.ToString()),
                "The first query is not the company list limited to the readable companies:\n" + first);
            Assert.DoesNotContain(capture.Commands, c => c.Contains("[dbo].[Access]") || c.Contains("[dbo].[RoleAccess]"));
        }

        // The company list takes the access service from the application's own registration.
        [Fact]
        public void The_application_registration_builds_the_company_list_service()
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

            Assert.IsType<CompanySelectService>(scope.ServiceProvider.GetRequiredService<ICompanySelectService>());
        }

        [Fact]
        public async Task The_company_list_is_unfiltered_for_every_company_and_empty_for_none()
        {
            var (every, everyCapture, _) = SelectService(ReadableCompanies.Every);
            var (none, noneCapture, noneReadable) = SelectService(ReadableCompanies.None);

            await CallSelect(every);
            var noneResult = await CallSelect(none);

            var first = everyCapture.Commands.FirstOrDefault() ?? "(no command sent)";
            Assert.True(first.Contains("FROM [dbo].[Company]") && !first.Contains("WHERE"),
                "Every company should need no company filter:\n" + first);
            Assert.True(noneReadable.Calls == 1 && noneCapture.Commands.Count == 0 && noneResult is IEnumerable<KSS.Dto.CompanySelectDto> list && !list.Any(),
                "A caller who may read no company should get an empty list without any query.");
        }
    }
}
