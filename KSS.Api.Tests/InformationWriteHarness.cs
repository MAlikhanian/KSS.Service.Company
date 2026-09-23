using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using KSS.Data.DbContexts;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Repository.IRepository;
using KSS.Service.IService;
using KSS.Service.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace KSS.Api.Tests
{
    /// <summary>Thrown the moment code tries to open a database connection.</summary>
    public sealed class DatabaseReachedException : Exception
    {
        public DatabaseReachedException() : base("The code reached the database.") { }
    }

    /// <summary>
    /// No database is available to these tests. Every attempt to open a connection throws
    /// <see cref="DatabaseReachedException"/> before any network activity, so a test can
    /// tell "stopped before the database" from "went on to the database".
    /// </summary>
    public sealed class DatabaseSentinel : DbConnectionInterceptor
    {
        public int Attempts { get; private set; }

        public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            Attempts++;
            throw new DatabaseReachedException();
        }

        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            Attempts++;
            throw new DatabaseReachedException();
        }

        public static bool Reached(Exception? exception)
        {
            for (var e = exception; e != null; e = e.InnerException)
                if (e is DatabaseReachedException) return true;
            return false;
        }
    }

    /// <summary>In-memory rows and a write log for the name-management dependencies.</summary>
    public sealed class NameStore
    {
        public List<NameHistory> Histories { get; } = new();
        public List<NameHistoryTranslation> HistoryTranslations { get; } = new();
        public List<Translation> CompanyTranslations { get; } = new();
        public List<string> Writes { get; } = new();
    }

    /// <summary>
    /// One proxy type for the five name-management dependencies. Reads are answered from
    /// the store, writes are logged, and any other call throws so an unexpected data
    /// access fails the test instead of passing silently.
    /// </summary>
    public class NameFake : DispatchProxy
    {
        private NameStore _store = null!;
        private string _role = string.Empty;

        public static T Create<T>(NameStore store, string role) where T : class
        {
            var proxy = Create<T, NameFake>();
            var state = (NameFake)(object)proxy;
            state._store = store;
            state._role = role;
            return proxy;
        }

        private static IQueryable<TRow> Filter<TRow>(IEnumerable<TRow> rows, object? expression) =>
            rows.AsQueryable().Where((Expression<Func<TRow, bool>>)expression!);

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            var name = method!.Name;
            switch (_role, name)
            {
                case ("history", "SingleOrDefault"): return Filter(_store.Histories, args![0]).SingleOrDefault();
                case ("history", "ToList"): return Filter(_store.Histories, args![0]).ToList();

                case ("historyTranslation", "SingleOrDefault"): return Filter(_store.HistoryTranslations, args![0]).SingleOrDefault();
                case ("historyTranslation", "ToList"): return Filter(_store.HistoryTranslations, args![0]).ToList();
                case ("historyTranslation", "Count"): return Filter(_store.HistoryTranslations, args![0]).Count();

                case ("companyTranslation", "SingleOrDefault"): return Filter(_store.CompanyTranslations, args![0]).SingleOrDefault();
                case ("companyTranslation", "Add"):
                case ("companyTranslation", "Update"):
                case ("companyTranslation", "Remove"):
                    _store.Writes.Add("Translation." + name);
                    return null;

                case ("historyService", "AddNameHistoryDtoAsync"):
                    _store.Writes.Add("NameHistory.Add");
                    return Task.FromResult(ServiceResult.Ok());
                case ("historyService", "DeleteNameHistory"):
                    _store.Writes.Add("NameHistory.Delete");
                    return ServiceResult.Ok();

                case ("historyTranslationService", "AddDtoAsync"):
                    _store.Writes.Add("NameHistoryTranslation.Add");
                    return Task.CompletedTask;
                case ("historyTranslationService", "UpdateDto"):
                case ("historyTranslationService", "Remove"):
                    _store.Writes.Add("NameHistoryTranslation." + name);
                    return null;
            }

            throw new NotSupportedException("Unexpected call: " + _role + "." + name);
        }
    }

    /// <summary>
    /// Builds the three services the way the application does (by constructor injection),
    /// on a real <see cref="MainDbContext"/> whose connection is guarded by
    /// <see cref="DatabaseSentinel"/>. Rows a test needs are attached to the change
    /// tracker, so a lookup by key is answered without a query.
    /// </summary>
    public sealed class InformationWriteHarness : IDisposable
    {
        public static readonly Guid OwnCompany = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid OtherCompany = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid Caller = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public DatabaseSentinel Sentinel { get; } = new();
        public MainDbContext Db { get; }
        public FakeAccessService Access { get; }
        public NameStore Names { get; } = new();
        public ICompanyContactService Contacts { get; }
        public ICompanySoftwareManagementService Software { get; }
        public ICompanyNameManagementService NameManagement { get; }

        public InformationWriteHarness(bool withCaller = true)
        {
            var http = new DefaultHttpContext();
            if (withCaller)
                http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("personId", Caller.ToString()) }, "test"));
            var accessor = new HttpContextAccessor { HttpContext = http };

            var options = new DbContextOptionsBuilder<MainDbContext>()
                .UseSqlServer("Server=database.not.available.invalid;Database=none;Encrypt=False")
                .AddInterceptors(Sentinel)
                .Options;
            Db = new MainDbContext(options, accessor);

            var (accessService, accessState) = FakeAccessService.Create();
            Access = accessState;

            var services = new ServiceCollection();
            services.AddSingleton(Db);
            services.AddSingleton(accessService);
            services.AddSingleton<IHttpContextAccessor>(accessor);
            services.AddSingleton(NameFake.Create<INameHistoryService>(Names, "historyService"));
            services.AddSingleton(NameFake.Create<INameHistoryTranslationService>(Names, "historyTranslationService"));
            services.AddSingleton(NameFake.Create<INameHistoryTranslationRepository>(Names, "historyTranslation"));
            services.AddSingleton(NameFake.Create<INameHistoryRepository>(Names, "history"));
            services.AddSingleton(NameFake.Create<ITranslationRepository>(Names, "companyTranslation"));
            var provider = services.BuildServiceProvider();

            Contacts = ActivatorUtilities.CreateInstance<CompanyContactService>(provider);
            Software = ActivatorUtilities.CreateInstance<CompanySoftwareManagementService>(provider);
            NameManagement = ActivatorUtilities.CreateInstance<CompanyNameManagementService>(provider);
        }

        public Guid StoredEmail(Guid companyId) => Attach(new Email { Id = Guid.NewGuid(), CompanyId = companyId, EmailAddress = "stored@example.test" }).Id;
        public Guid StoredPhone(Guid companyId) => Attach(new Phone { Id = Guid.NewGuid(), CompanyId = companyId, PhoneNumber = "000" }).Id;
        public Guid StoredAddress(Guid companyId) => Attach(new Address { Id = Guid.NewGuid(), CompanyId = companyId, PostalCode = "000" }).Id;
        public Guid StoredWebsite(Guid companyId) => Attach(new Website { Id = Guid.NewGuid(), CompanyId = companyId, Url = "https://stored.example.test" }).Id;

        /// <summary>A current name history entry with two translations.</summary>
        public Guid StoredNameHistory(Guid companyId)
        {
            var history = new NameHistory { Id = Guid.NewGuid(), CompanyId = companyId, StartDate = new DateTime(2020, 1, 1) };
            Names.Histories.Add(history);
            Names.HistoryTranslations.Add(new NameHistoryTranslation { NameHistoryId = history.Id, LanguageId = 12, Name = "Stored name" });
            Names.HistoryTranslations.Add(new NameHistoryTranslation { NameHistoryId = history.Id, LanguageId = 10, Name = "Stored name" });
            return history.Id;
        }

        private T Attach<T>(T entity) where T : class
        {
            Db.Attach(entity);
            return entity;
        }

        public void Dispose() => Db.Dispose();
    }
}
