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
    /// <summary>Maps the two translation DTOs to their entities; anything else throws.</summary>
    public class TranslationMapper : DispatchProxy
    {
        public static IMapper Make() => Create<IMapper, TranslationMapper>();

        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == "Map" && method.IsGenericMethod && args!.Length == 1)
            {
                if (method.ReturnType == typeof(Translation) && args[0] is TranslationDto t)
                    return new Translation { CompanyId = t.CompanyId, LanguageId = t.LanguageId, Name = t.Name, ShortName = t.ShortName, Description = t.Description };
                if (method.ReturnType == typeof(NameHistoryTranslation) && args[0] is NameHistoryTranslationDto n)
                    return new NameHistoryTranslation { NameHistoryId = n.NameHistoryId, LanguageId = n.LanguageId, Name = n.Name, ShortName = n.ShortName };
            }

            throw new NotSupportedException("Unexpected mapper call: " + method.Name);
        }
    }

    /// <summary>
    /// Company-name writes reached through BaseController on the Translation and
    /// NameHistoryTranslation controllers. Every write action must check the caller's
    /// Information level 2 on the company it changes: the row's company for Translation,
    /// and the company of the stored name history entry for NameHistoryTranslation.
    /// Actions are invoked on the real controllers, as routed.
    /// </summary>
    public class TranslationWriteScopeTests
    {
        private static readonly Guid Own = Guid.Parse("77777777-7777-7777-7777-777777777777");
        private static readonly Guid Other = Guid.Parse("88888888-8888-8888-8888-888888888888");
        private static readonly Guid Caller = Guid.Parse("99999999-9999-9999-9999-999999999999");

        private sealed class Harness
        {
            public FakeAccessService Access { get; }
            public RowRepository<Translation> TranslationRows { get; }
            public RowRepository<NameHistoryTranslation> EntryTranslationRows { get; }
            public RowRepository<NameHistory> EntryRows { get; }
            public ITranslationService TranslationService { get; }
            public INameHistoryTranslationService EntryTranslationService { get; }
            public TranslationController Translations { get; }
            public NameHistoryTranslationController EntryTranslations { get; }

            public Harness(bool withCaller = true)
            {
                var http = new DefaultHttpContext();
                if (withCaller)
                    http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("personId", Caller.ToString()) }, "test"));

                var (accessService, access) = FakeAccessService.Create();
                Access = access;
                var (translationRepository, translationRows) = RowRepository<Translation>.Make<ITranslationRepository>(_ => Guid.Empty);
                var (entryTranslationRepository, entryTranslationRows) = RowRepository<NameHistoryTranslation>.Make<INameHistoryTranslationRepository>(_ => Guid.Empty);
                var (entryRepository, entryRows) = RowRepository<NameHistory>.Make<INameHistoryRepository>(r => r.Id);
                TranslationRows = translationRows;
                EntryTranslationRows = entryTranslationRows;
                EntryRows = entryRows;

                var services = new ServiceCollection();
                services.AddSingleton(accessService);
                services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = http });
                services.AddSingleton(TranslationMapper.Make());
                services.AddSingleton(translationRepository);
                services.AddSingleton(entryTranslationRepository);
                services.AddSingleton(entryRepository);
                var provider = services.BuildServiceProvider();

                TranslationService = ActivatorUtilities.CreateInstance<TranslationService>(provider);
                EntryTranslationService = ActivatorUtilities.CreateInstance<NameHistoryTranslationService>(provider);
                Translations = new TranslationController(TranslationService);
                EntryTranslations = new NameHistoryTranslationController(EntryTranslationService);
            }

            public int WriteCount => TranslationRows.Writes.Count + EntryTranslationRows.Writes.Count + EntryRows.Writes.Count;

            public void StoredTranslation(Guid companyId) =>
                TranslationRows.Rows.Add(new Translation { CompanyId = companyId, LanguageId = 12, Name = "Stored" });

            /// <summary>A name history entry of the company; with a stored translation when asked.</summary>
            public Guid StoredEntry(Guid companyId, bool withTranslation = false)
            {
                var entry = new NameHistory { Id = Guid.NewGuid(), CompanyId = companyId, StartDate = new DateTime(2020, 1, 1) };
                EntryRows.Rows.Add(entry);
                if (withTranslation)
                    EntryTranslationRows.Rows.Add(new NameHistoryTranslation { NameHistoryId = entry.Id, LanguageId = 12, Name = "Stored" });
                return entry.Id;
            }
        }

        private sealed record WriteCase(string Name, Func<Harness, Guid, Task> Run);

        private static Task Sync(Action action)
        {
            action();
            return Task.CompletedTask;
        }

        private static Translation Tr(Guid companyId, short language = 12) => new() { CompanyId = companyId, LanguageId = language, Name = "Name" };

        private static NameHistoryTranslation EntryTr(Guid entryId, short language = 12) => new() { NameHistoryId = entryId, LanguageId = language, Name = "Name" };

        // Every BaseController write action, on both controllers, keyed Controller.Action.
        private static readonly WriteCase[] Cases =
        {
            new("Translation.AddAsync", (h, c) => h.Translations.AddAsync(Tr(c, 30))),
            new("Translation.AddDtoAsync", (h, c) => h.Translations.AddDtoAsync(new TranslationDto { CompanyId = c, LanguageId = 30, Name = "Name" })),
            new("Translation.AddRangeAsync", (h, c) => h.Translations.AddRangeAsync(new[] { Tr(c, 30), Tr(c, 31) })),
            new("Translation.Update", (h, c) => { h.StoredTranslation(c); return Sync(() => h.Translations.Update(Tr(c))); }),
            new("Translation.UpdateDto", (h, c) => { h.StoredTranslation(c); return Sync(() => h.Translations.UpdateDto(new TranslationDto { CompanyId = c, LanguageId = 12, Name = "Renamed" })); }),
            new("Translation.UpdateRange", (h, c) => { h.StoredTranslation(c); return Sync(() => h.Translations.UpdateRange(new[] { Tr(c) })); }),
            new("Translation.Remove", (h, c) => { h.StoredTranslation(c); return Sync(() => h.Translations.Remove(Tr(c))); }),
            new("Translation.RemoveRange", (h, c) => { h.StoredTranslation(c); return Sync(() => h.Translations.RemoveRange(new[] { Tr(c) })); }),
            new("NameHistoryTranslation.AddAsync", (h, c) => h.EntryTranslations.AddAsync(EntryTr(h.StoredEntry(c), 30))),
            new("NameHistoryTranslation.AddDtoAsync", (h, c) => h.EntryTranslations.AddDtoAsync(new NameHistoryTranslationDto { NameHistoryId = h.StoredEntry(c), LanguageId = 30, Name = "Name" })),
            new("NameHistoryTranslation.AddRangeAsync", (h, c) => h.EntryTranslations.AddRangeAsync(new[] { EntryTr(h.StoredEntry(c), 30) })),
            new("NameHistoryTranslation.Update", (h, c) => Sync(() => h.EntryTranslations.Update(EntryTr(h.StoredEntry(c, withTranslation: true))))),
            new("NameHistoryTranslation.UpdateDto", (h, c) => Sync(() => h.EntryTranslations.UpdateDto(new NameHistoryTranslationDto { NameHistoryId = h.StoredEntry(c, withTranslation: true), LanguageId = 12, Name = "Renamed" }))),
            new("NameHistoryTranslation.UpdateRange", (h, c) => Sync(() => h.EntryTranslations.UpdateRange(new[] { EntryTr(h.StoredEntry(c, withTranslation: true)) }))),
            new("NameHistoryTranslation.Remove", (h, c) => Sync(() => h.EntryTranslations.Remove(EntryTr(h.StoredEntry(c, withTranslation: true))))),
            new("NameHistoryTranslation.RemoveRange", (h, c) => Sync(() => h.EntryTranslations.RemoveRange(new[] { EntryTr(h.StoredEntry(c, withTranslation: true)) }))),
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
            // Reads are POST too; the application's own permission filter classifies actions.
            var map = (IReadOnlyDictionary<string, string>)typeof(KSS.Helper.Authorization.PermissionAuthorizationFilter)
                .GetField("ActionToOperation", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null)!;
            var writes = typeof(BaseController<,,,>)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes(inherit: true).OfType<HttpMethodAttribute>().Any())
                .Select(m => m.Name)
                .Where(n => !(map.TryGetValue(n, out var operation) && operation == "Read"))
                .ToHashSet();

            foreach (var controller in new[] { "Translation", "NameHistoryTranslation" })
            {
                var swept = Cases.Where(c => c.Name.StartsWith(controller + ".", StringComparison.Ordinal))
                    .Select(c => c.Name.Substring(controller.Length + 1))
                    .ToHashSet();
                Assert.True(writes.SetEquals(swept), controller + ": sweep " + string.Join(",", swept.OrderBy(x => x)) + " vs base " + string.Join(",", writes.OrderBy(x => x)));
            }
        }

        // ── Name history translations: the stored entry decides the company ────────

        [Fact]
        public async Task A_translation_of_another_companys_entry_is_refused_whatever_the_caller_holds_elsewhere()
        {
            var h = new Harness();
            h.Access.InformationLevels[Own] = 2;

            var attempts = new (string Name, Func<Task> Run)[]
            {
                ("AddDtoAsync", () => h.EntryTranslations.AddDtoAsync(new NameHistoryTranslationDto { NameHistoryId = h.StoredEntry(Other), LanguageId = 30, Name = "x" })),
                ("UpdateDto", () => Sync(() => h.EntryTranslations.UpdateDto(new NameHistoryTranslationDto { NameHistoryId = h.StoredEntry(Other, withTranslation: true), LanguageId = 12, Name = "x" }))),
                ("Remove", () => Sync(() => h.EntryTranslations.Remove(EntryTr(h.StoredEntry(Other, withTranslation: true))))),
            };

            foreach (var (name, run) in attempts)
            {
                var error = await Record.ExceptionAsync(run);
                Assert.True(IsModifyDenial(error), name + " was not refused; outcome: " + Outcome(error));
            }

            Assert.Equal(0, h.WriteCount);
            Assert.All(h.Access.LevelQueries, q => Assert.Equal(Other, q.CompanyId));
        }

        [Fact]
        public async Task A_translation_for_an_unknown_entry_is_refused()
        {
            var h = new Harness();
            h.Access.InformationLevels[Own] = 2;

            var error = await Record.ExceptionAsync(() => h.EntryTranslations.AddDtoAsync(new NameHistoryTranslationDto { NameHistoryId = Guid.NewGuid(), LanguageId = 12, Name = "x" }));

            Assert.True(error is BusinessRuleException, "outcome: " + Outcome(error));
            Assert.Equal(0, h.WriteCount);
        }

        [Theory]
        [InlineData("Translation.AddDtoAsync")]
        [InlineData("Translation.UpdateDto")]
        [InlineData("NameHistoryTranslation.AddDtoAsync")]
        [InlineData("NameHistoryTranslation.Remove")]
        public async Task A_request_without_a_caller_is_refused(string name)
        {
            var h = new Harness(withCaller: false);
            h.Access.InformationLevels[Other] = 2;

            var error = await Record.ExceptionAsync(() => Case(name).Run(h, Other));

            Assert.True(error is BusinessRuleException && error.Message.Contains("PersonId", StringComparison.Ordinal),
                name + " did not fail closed; outcome: " + Outcome(error));
            Assert.Equal(0, h.WriteCount);
        }

        // ── Company creation: the unguarded paths exist only for the creation service ──

        [Theory]
        [InlineData("KSS.Service.Service.INewCompanyTranslation", typeof(TranslationService), typeof(ITranslationService))]
        [InlineData("KSS.Service.Service.INewCompanyNameHistoryTranslation", typeof(NameHistoryTranslationService), typeof(INameHistoryTranslationService))]
        public void The_creation_path_is_internal_and_not_on_any_public_surface(string interfaceName, Type service, Type publicInterface)
        {
            var creation = service.Assembly.GetType(interfaceName);

            Assert.NotNull(creation);
            Assert.False(creation!.IsPublic || creation.IsNestedPublic);
            Assert.DoesNotContain(service.GetMethods(BindingFlags.Public | BindingFlags.Instance), m => m.Name.Contains("AddForNewCompany", StringComparison.Ordinal));
            Assert.DoesNotContain(publicInterface.GetMethods(), m => m.Name.Contains("AddForNewCompany", StringComparison.Ordinal));
        }

        [Fact]
        public async Task The_creation_paths_add_without_consulting_the_access_service()
        {
            var h = new Harness();
            var assembly = typeof(TranslationService).Assembly;
            var translation = assembly.GetType("KSS.Service.Service.INewCompanyTranslation")!.GetMethod("AddForNewCompanyAsync")!;
            var entryTranslation = assembly.GetType("KSS.Service.Service.INewCompanyNameHistoryTranslation")!.GetMethod("AddForNewCompanyAsync")!;

            await (Task)translation.Invoke(h.TranslationService, new object[] { new TranslationDto { CompanyId = Other, LanguageId = 12, Name = "New" } })!;
            await (Task)entryTranslation.Invoke(h.EntryTranslationService, new object[] { new NameHistoryTranslationDto { NameHistoryId = Guid.NewGuid(), LanguageId = 12, Name = "New" } })!;

            Assert.Contains("AddAsync", h.TranslationRows.Writes);
            Assert.Contains("AddAsync", h.EntryTranslationRows.Writes);
            Assert.Empty(h.Access.LevelQueries);
        }

        // Finds, by IL, every type whose code calls the named method.
        private static HashSet<string> CallersOf(string declaringTypeName, string methodName)
        {
            var callers = new HashSet<string>();
            var assemblies = new[] { typeof(TranslationService).Assembly, typeof(TranslationController).Assembly };
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
            {
                foreach (var body in type.GetMethods(all).Cast<MethodBase>().Concat(type.GetConstructors(all)))
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

        [Theory]
        [InlineData("INewCompanyTranslation")]
        [InlineData("INewCompanyNameHistoryTranslation")]
        public void Only_the_company_creation_service_calls_the_creation_path(string interfaceName)
        {
            Assert.Equal(new[] { "CompanyOperationService" }, CallersOf(interfaceName, "AddForNewCompanyAsync").OrderBy(x => x).ToArray());
        }
    }
}
