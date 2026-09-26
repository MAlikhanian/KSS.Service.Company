using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using KSS.Api.Controller;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Helper.Authorization;
using KSS.Helper.CustomAttribute;
using KSS.Repository.IRepository;
using KSS.Repository.Repository;
using KSS.Service.IService;
using KSS.Service.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KSS.Api.Tests
{
    /// <summary>In-memory rows answering reads through the caller's own predicate; every read is counted.</summary>
    public class OwnRows<T> : DispatchProxy where T : class
    {
        public List<T> Rows { get; } = new();
        public int Reads { get; private set; }

        public static (TRepository Repository, OwnRows<T> State) Make<TRepository>() where TRepository : class
        {
            var proxy = Create<TRepository, OwnRows<T>>();
            return (proxy, (OwnRows<T>)(object)proxy);
        }

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == "ToListAsync" && args!.Length == 1 && args[0] is LambdaExpression filter)
            {
                Reads++;
                return Task.FromResult<IEnumerable<T>>(Rows.AsQueryable().Where((Expression<Func<T, bool>>)filter).ToList());
            }
            throw new NotSupportedException("Unexpected repository call: " + method.Name);
        }
    }

    /// <summary>An access service that must not be used by the caller's own-grants route.</summary>
    public class UnusedAccessService : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? method, object?[]? args) =>
            throw new NotSupportedException("The own-grants route used the access service: " + method!.Name);
    }

    /// <summary>
    /// GET Api/Access/MyGrants: the signed-in caller's own grants, personal and through her roles,
    /// live or not, and the companies they name. Estate-wide grants name no company and are never
    /// expanded. The caller and her roles come from the token only.
    /// </summary>
    public class MyGrantsTests
    {
        private static readonly Guid Caller = Guid.Parse("11111111-bbbb-0000-0000-000000000001");
        private static readonly Guid SomeoneElse = Guid.Parse("22222222-bbbb-0000-0000-000000000002");
        private static readonly Guid CallerRole = Guid.Parse("99999999-bbbb-0000-0000-000000000001");
        private static readonly Guid OtherRole = Guid.Parse("99999999-bbbb-0000-0000-000000000002");
        private static readonly Guid CompanyA = Guid.Parse("aaaaaaaa-bbbb-0000-0000-00000000000a");
        private static readonly Guid CompanyB = Guid.Parse("bbbbbbbb-bbbb-0000-0000-00000000000b");
        private static readonly Guid CompanyC = Guid.Parse("cccccccc-bbbb-0000-0000-00000000000c");
        private static readonly Guid CompanyD = Guid.Parse("dddddddd-bbbb-0000-0000-00000000000d");
        private static readonly Guid CompanyE = Guid.Parse("eeeeeeee-bbbb-0000-0000-00000000000e");
        private static readonly Guid CompanyF = Guid.Parse("ffffffff-bbbb-0000-0000-00000000000f");
        private static readonly DateTime Deleted = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private sealed class Setup
        {
            public OwnRows<Access> Access { get; }
            public OwnRows<RoleAccess> RoleAccess { get; }
            public OwnRows<Company> Companies { get; }
            public OwnRows<Translation> Translations { get; }
            public IMyGrantsService Service { get; }

            public Setup()
            {
                var (access, accessRows) = OwnRows<Access>.Make<IAccessRepository>();
                var (roles, roleRows) = OwnRows<RoleAccess>.Make<IRoleAccessRepository>();
                var (companies, companyRows) = OwnRows<Company>.Make<ICompanyRepository>();
                var (translations, translationRows) = OwnRows<Translation>.Make<ITranslationRepository>();
                Access = accessRows;
                RoleAccess = roleRows;
                Companies = companyRows;
                Translations = translationRows;
                Service = new MyGrantsService(access, roles, companies, translations);
            }

            public int Reads => Access.Reads + RoleAccess.Reads + Companies.Reads + Translations.Reads;

            public AccessController Controller(ClaimsPrincipal user) => new(DispatchProxy.Create<IAccessService, UnusedAccessService>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } },
            };
        }

        private static ClaimsPrincipal Signed(Guid? person, params Guid[] roles) => new(new ClaimsIdentity(
            (person.HasValue ? new[] { new Claim("personId", person.Value.ToString()) } : Array.Empty<Claim>())
                .Concat(roles.Select(r => new Claim("roleId", r.ToString()))), "test"));

        private static Access Grant(Guid company, Guid person, byte section, int level, bool active = true, DateTime? deleted = null) =>
            new() { Id = Guid.NewGuid(), CompanyId = company, GrantedToPersonId = person, SectionId = section, Level = level, IsActive = active, DeletedAt = deleted };

        private static RoleAccess RoleGrant(Guid? company, Guid role, byte section, int level, bool active = true, DateTime? deleted = null) =>
            new() { Id = Guid.NewGuid(), CompanyId = company, GrantedToRoleId = role, SectionId = section, Level = level, IsActive = active, DeletedAt = deleted };

        private static Company CompanyRow(Guid id, string nationalId, bool active = true, DateTime? deleted = null) =>
            new() { Id = id, NationalId = nationalId, RegistrationNo = "1", IsActive = active, DeletedAt = deleted };

        private static async Task<MyGrantsDto> Get(Setup s, ClaimsPrincipal user)
        {
            var result = await s.Controller(user).MyGrants(s.Service);
            return Assert.IsType<MyGrantsDto>(Assert.IsType<OkObjectResult>(result.Result).Value);
        }

        [Fact]
        public async Task Returns_only_the_callers_own_rows_and_the_rows_of_her_roles()
        {
            var s = new Setup();
            s.Access.Rows.AddRange(new[]
            {
                Grant(CompanyA, Caller, AccessSectionId.Information, 2),
                Grant(CompanyB, Caller, AccessSectionId.Access, 1, active: false),
                Grant(CompanyC, Caller, AccessSectionId.Information, 1, deleted: Deleted),
                Grant(CompanyA, SomeoneElse, AccessSectionId.Access, 2),
                Grant(CompanyD, SomeoneElse, AccessSectionId.Information, 2),
            });
            s.RoleAccess.Rows.AddRange(new[]
            {
                RoleGrant(null, CallerRole, AccessSectionId.Information, 1),
                RoleGrant(CompanyE, CallerRole, AccessSectionId.Information, 2),
                RoleGrant(CompanyF, OtherRole, AccessSectionId.Information, 2),
                RoleGrant(null, OtherRole, AccessSectionId.Access, 2),
            });
            foreach (var (id, n) in new[] { (CompanyA, "a"), (CompanyB, "b"), (CompanyC, "c"), (CompanyD, "d"), (CompanyE, "e"), (CompanyF, "f") })
                s.Companies.Rows.Add(CompanyRow(id, n));

            var mine = await Get(s, Signed(Caller, CallerRole));

            var rows = mine.Grants.Select(g => (g.CompanyId, g.SectionId, g.Level, g.IsActive, g.IsDeleted)).ToList();
            Assert.Equal(new (Guid?, byte, int, bool, bool)[]
            {
                (null, AccessSectionId.Information, 1, true, false),
                (CompanyA, AccessSectionId.Information, 2, true, false),
                (CompanyB, AccessSectionId.Access, 1, false, false),
                (CompanyC, AccessSectionId.Information, 1, true, true),
                (CompanyE, AccessSectionId.Information, 2, true, false),
            }, rows);
            Assert.Equal(new[] { CompanyA, CompanyB, CompanyC, CompanyE }, mine.Companies.Select(c => c.CompanyId).ToArray());
        }

        [Fact]
        public async Task An_estate_wide_grant_names_no_company_and_is_not_expanded()
        {
            var s = new Setup();
            s.RoleAccess.Rows.Add(RoleGrant(null, CallerRole, AccessSectionId.Information, 2));
            foreach (var id in new[] { CompanyA, CompanyB, CompanyC })
                s.Companies.Rows.Add(CompanyRow(id, "x"));

            var mine = await Get(s, Signed(Caller, CallerRole));

            var grant = Assert.Single(mine.Grants);
            Assert.Null(grant.CompanyId);
            Assert.Empty(mine.Companies);
            Assert.Equal(0, s.Companies.Reads);
            Assert.Equal(0, s.Translations.Reads);
        }

        // Names are every translation that is not deleted; a company with none returns an empty list.
        [Fact]
        public async Task The_company_block_carries_its_flags_and_names()
        {
            var s = new Setup();
            s.Access.Rows.AddRange(new[]
            {
                Grant(CompanyA, Caller, AccessSectionId.Information, 1),
                Grant(CompanyB, Caller, AccessSectionId.Information, 1),
                Grant(CompanyC, Caller, AccessSectionId.Information, 1),
            });
            s.Companies.Rows.AddRange(new[]
            {
                CompanyRow(CompanyA, "10000000001"),
                CompanyRow(CompanyB, "10000000002", active: false),
                CompanyRow(CompanyC, "10000000003", deleted: Deleted),
            });
            s.Translations.Rows.AddRange(new[]
            {
                new Translation { CompanyId = CompanyA, LanguageId = 12, Name = "نام شرکت", ShortName = "کوتاه" },
                new Translation { CompanyId = CompanyA, LanguageId = 10, Name = "Company A" },
                new Translation { CompanyId = CompanyB, LanguageId = 10, Name = "Company B" },
                new Translation { CompanyId = CompanyB, LanguageId = 12, Name = "deleted name", DeletedAt = Deleted },
                new Translation { CompanyId = CompanyD, LanguageId = 12, Name = "not granted" },
            });

            var mine = await Get(s, Signed(Caller));

            var a = mine.Companies.Single(c => c.CompanyId == CompanyA);
            Assert.True(a.IsActive && !a.IsDeleted);
            Assert.Equal(new (short, string, string?)[] { (10, "Company A", null), (12, "نام شرکت", "کوتاه") },
                a.Names.Select(n => (n.LanguageId, n.Name, n.ShortName)).ToArray());

            var b = mine.Companies.Single(c => c.CompanyId == CompanyB);
            Assert.True(!b.IsActive && !b.IsDeleted);
            Assert.Equal(new (short, string, string?)[] { (10, "Company B", null) }, b.Names.Select(n => (n.LanguageId, n.Name, n.ShortName)).ToArray());

            var deletedCompany = mine.Companies.Single(x => x.CompanyId == CompanyC);
            Assert.True(deletedCompany.IsDeleted);
            Assert.NotNull(deletedCompany.Names);
            Assert.Empty(deletedCompany.Names);

            Assert.DoesNotContain(mine.Companies, x => x.CompanyId == CompanyD);
        }

        [Fact]
        public async Task A_request_without_a_caller_is_refused_before_any_read()
        {
            var s = new Setup();
            s.Access.Rows.Add(Grant(CompanyA, SomeoneElse, AccessSectionId.Information, 2));

            var error = await Record.ExceptionAsync(() => s.Controller(Signed(null, CallerRole)).MyGrants(s.Service));

            Assert.True(error is BusinessRuleException && error.Message.Contains("PersonId", StringComparison.Ordinal), "outcome: " + error);
            Assert.Equal(0, s.Reads);
        }

        [Fact]
        public async Task A_caller_with_no_roles_reads_no_role_rows()
        {
            var s = new Setup();
            s.RoleAccess.Rows.Add(RoleGrant(null, OtherRole, AccessSectionId.Information, 2));

            var mine = await Get(s, Signed(Caller));

            Assert.Empty(mine.Grants);
            Assert.Equal(0, s.RoleAccess.Reads);
        }

        // Nothing in the request can name a person: the route's only input is the service,
        // supplied by the container; the request itself carries nothing.
        [Fact]
        public void Nothing_in_the_request_can_name_a_person()
        {
            var action = typeof(AccessController).GetMethod(nameof(AccessController.MyGrants))!;
            var parameter = Assert.Single(action.GetParameters());

            Assert.Equal(typeof(IMyGrantsService), parameter.ParameterType);
            Assert.NotNull(parameter.GetCustomAttribute<FromServicesAttribute>());
        }

        // Sign-in only, and outside both read gates: the permission filter is attached only to
        // BaseController and maps only its action names, and the generic-read refusal is a member
        // of BaseController, so neither is on this route.
        [Fact]
        public void The_route_is_sign_in_only_and_outside_both_read_gates()
        {
            var controller = typeof(AccessController);
            var action = controller.GetMethod(nameof(AccessController.MyGrants))!;

            Assert.NotEmpty(controller.GetCustomAttributes<AuthorizeAttribute>(inherit: true));
            Assert.Empty(action.GetCustomAttributes<HasPermissionAttribute>(inherit: true));
            Assert.Empty(controller.GetCustomAttributes<HasPermissionAttribute>(inherit: true));
            Assert.Empty(action.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true));
            Assert.Null(controller.GetCustomAttribute<PermissionGroupAttribute>(inherit: true));
            Assert.DoesNotContain(controller.GetCustomAttributes<ServiceFilterAttribute>(inherit: true), f => f.ServiceType == typeof(PermissionAuthorizationFilter));
            Assert.False(controller.BaseType is { IsGenericType: true } b && b.GetGenericTypeDefinition() == typeof(BaseController<,,,>));
            Assert.NotNull(action.GetCustomAttribute<HttpGetAttribute>());
            Assert.Null(action.GetCustomAttribute<HttpGetAttribute>()!.Template);
            Assert.Equal("Api/[controller]/[action]", controller.GetCustomAttribute<RouteAttribute>()!.Template);
        }

        [Fact]
        public void The_application_registration_resolves_the_service()
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

            Assert.IsType<MyGrantsService>(scope.ServiceProvider.GetRequiredService<IMyGrantsService>());
        }

        // The field names are the contract the consumer builds against.
        [Fact]
        public void The_response_json_uses_the_contract_field_names()
        {
            var dto = new MyGrantsDto
            {
                Grants = { new MyGrantDto { CompanyId = null, SectionId = 1, Level = 1, IsActive = true, IsDeleted = false } },
                Companies = { new MyGrantCompanyDto { CompanyId = CompanyA, Names = { new CompanyNameDto { LanguageId = 12, Name = "n" } } } },
            };

            using var json = JsonDocument.Parse(JsonSerializer.Serialize(dto, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            string[] Keys(JsonElement e) => e.EnumerateObject().Select(p => p.Name).OrderBy(x => x, StringComparer.Ordinal).ToArray();

            Assert.Equal(new[] { "companies", "grants" }, Keys(json.RootElement));
            Assert.Equal(new[] { "companyId", "isActive", "isDeleted", "level", "sectionId" }, Keys(json.RootElement.GetProperty("grants")[0]));
            Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("grants")[0].GetProperty("companyId").ValueKind);
            var company = json.RootElement.GetProperty("companies")[0];
            Assert.Equal(new[] { "companyId", "isActive", "isDeleted", "names" }, Keys(company));
            Assert.Equal(new[] { "languageId", "name", "shortName" }, Keys(company.GetProperty("names")[0]));
        }

        [Fact]
        public async Task The_personal_grant_query_is_keyed_on_the_caller_in_sql()
        {
            var capture = new CommandCapture();
            using var db = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>()
                .UseSqlServer("Server=database.not.available.invalid;Database=none;Encrypt=False")
                .AddInterceptors(new NoOpenConnection(), capture).Options, new HttpContextAccessor());
            var service = new MyGrantsService(new AccessRepository(db), new RoleAccessRepository(db), new CompanyRepository(db), new TranslationRepository(db));

            try { await service.GetAsync(Caller, Array.Empty<Guid>()); } catch (InvalidOperationException) { }

            var sql = capture.Commands.FirstOrDefault() ?? "(no command sent)";
            Assert.True(sql.Contains("FROM [dbo].[Access]") && sql.Contains("[GrantedToPersonId]") && sql.Contains("WHERE") && sql.Contains(Caller.ToString()),
                "The personal grant query is not keyed on the caller in SQL:\n" + sql);
        }
    }
}
