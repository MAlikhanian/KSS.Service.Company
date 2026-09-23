using System.Reflection;
using KSS.Api.Controller;
using KSS.Dto;
using KSS.Helper;
using KSS.Helper.CustomAttribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KSS.Api.Tests
{
    /// <summary>
    /// Company-information writes outside BaseController: every one must check the
    /// caller's Information level 2 on the company it changes. For an add that is the
    /// target company; for an update or delete it is the company of the stored row.
    /// Services are called through their interfaces, as the controllers call them.
    /// </summary>
    public class InformationWriteScopeTests
    {
        private static readonly Guid Own = InformationWriteHarness.OwnCompany;
        private static readonly Guid Other = InformationWriteHarness.OtherCompany;

        private enum Kind { Database, NameStore }

        private sealed record WriteCase(string Name, Kind Kind, Func<InformationWriteHarness, Guid, Task> Run);

        // Every company-information write reachable outside BaseController, keyed as
        // Controller.Action. The run delegate performs it against the given company.
        private static readonly WriteCase[] Cases =
        {
            new("CompanyContact.AddEmail", Kind.Database, (h, c) => h.Contacts.AddEmailAsync(c, new CompanyEmailInsertDto { EmailAddress = "new@example.test" })),
            new("CompanyContact.UpdateEmail", Kind.Database, (h, c) => h.Contacts.UpdateEmailAsync(h.StoredEmail(c), new CompanyEmailViewDto { EmailAddress = "new@example.test" })),
            new("CompanyContact.DeleteEmail", Kind.Database, (h, c) => h.Contacts.DeleteEmailAsync(h.StoredEmail(c))),
            new("CompanyContact.AddPhone", Kind.Database, (h, c) => h.Contacts.AddPhoneAsync(c, new CompanyPhoneInsertDto { PhoneNumber = "111" })),
            new("CompanyContact.UpdatePhone", Kind.Database, (h, c) => h.Contacts.UpdatePhoneAsync(h.StoredPhone(c), new CompanyPhoneViewDto { PhoneNumber = "111" })),
            new("CompanyContact.DeletePhone", Kind.Database, (h, c) => h.Contacts.DeletePhoneAsync(h.StoredPhone(c))),
            new("CompanyContact.AddAddress", Kind.Database, (h, c) => h.Contacts.AddAddressAsync(c, new CompanyAddressInsertDto { PostalCode = "111" })),
            new("CompanyContact.UpdateAddress", Kind.Database, (h, c) => h.Contacts.UpdateAddressAsync(h.StoredAddress(c), new CompanyAddressViewDto { PostalCode = "111" })),
            new("CompanyContact.DeleteAddress", Kind.Database, (h, c) => h.Contacts.DeleteAddressAsync(h.StoredAddress(c))),
            new("CompanyContact.AddWebsite", Kind.Database, (h, c) => h.Contacts.AddWebsiteAsync(c, new CompanyWebsiteInsertDto { Url = "https://new.example.test" })),
            new("CompanyContact.UpdateWebsite", Kind.Database, (h, c) => h.Contacts.UpdateWebsiteAsync(h.StoredWebsite(c), new CompanyWebsiteViewDto { Url = "https://new.example.test" })),
            new("CompanyContact.DeleteWebsite", Kind.Database, (h, c) => h.Contacts.DeleteWebsiteAsync(h.StoredWebsite(c))),
            new("CompanySoftware.Upsert", Kind.Database, (h, c) => h.Software.UpsertAsync(c, new CompanySoftwareUpsertDto { SoftwareCategoryId = 1, SoftwareId = 1 })),
            new("CompanySoftware.Clear", Kind.Database, (h, c) => h.Software.ClearAsync(c, 1)),
            new("CompanyNameManagement.AddNameWithTranslations", Kind.NameStore, (h, c) => h.NameManagement.AddNameWithTranslationsAsync(new AddNameWithTranslationsDto
            {
                CompanyId = c,
                StartDate = new DateTime(2024, 1, 1),
                Translations = new List<NameHistoryTranslationInsertDto> { new() { LanguageId = 12, Name = "New name" } },
            })),
            new("CompanyNameManagement.UpsertTranslations", Kind.NameStore, (h, c) => h.NameManagement.UpsertTranslationsAsync(new UpsertNameTranslationsDto
            {
                NameHistoryId = h.StoredNameHistory(c),
                CompanyId = c,
                Translations = new List<NameHistoryTranslationDto> { new() { LanguageId = 12, Name = "Renamed" } },
            })),
            new("CompanyNameManagement.DeleteNameHistory", Kind.NameStore, (h, c) =>
            {
                h.NameManagement.DeleteNameHistory(h.StoredNameHistory(c), c);
                return Task.CompletedTask;
            }),
            new("CompanyNameManagement.RemoveTranslation", Kind.NameStore, (h, c) =>
            {
                h.NameManagement.RemoveTranslation(new RemoveTranslationDto { NameHistoryId = h.StoredNameHistory(c), LanguageId = 12 });
                return Task.CompletedTask;
            }),
        };

        public static IEnumerable<object[]> CaseNames() => Cases.Select(c => new object[] { c.Name });

        private static WriteCase Case(string name) => Cases.Single(c => c.Name == name);

        private static bool IsModifyDenial(Exception? e) =>
            e is BusinessRuleException && e.Message.Contains("do not have permission to modify this company's", StringComparison.Ordinal);

        // ── The sweep: below level 2 on the company, every write is refused ───────

        [Theory]
        [MemberData(nameof(CaseNames))]
        public async Task A_caller_without_level_2_on_the_company_is_refused_before_any_write(string name)
        {
            using var h = new InformationWriteHarness();
            h.Access.InformationLevels[Other] = 1;

            var error = await Record.ExceptionAsync(() => Case(name).Run(h, Other));

            Assert.True(IsModifyDenial(error), name + " was not refused; outcome: " + (error?.GetType().Name ?? "completed"));
            Assert.Equal(0, h.Sentinel.Attempts);
            Assert.Empty(h.Names.Writes);
            Assert.Contains(h.Access.LevelQueries, q => q.CompanyId == Other && q.CallerPersonId == InformationWriteHarness.Caller);
        }

        [Theory]
        [MemberData(nameof(CaseNames))]
        public async Task A_caller_with_level_2_on_the_company_reaches_the_write(string name)
        {
            using var h = new InformationWriteHarness();
            h.Access.InformationLevels[Own] = 2;
            var writeCase = Case(name);

            var error = await Record.ExceptionAsync(() => writeCase.Run(h, Own));

            Assert.False(IsModifyDenial(error), name + " was refused at level 2.");
            if (writeCase.Kind == Kind.Database)
                Assert.True(DatabaseSentinel.Reached(error), name + " did not go on to the database; outcome: " + (error?.GetType().Name ?? "completed"));
            else
            {
                Assert.Null(error);
                Assert.NotEmpty(h.Names.Writes);
            }
        }

        // ── Updates and deletes: the stored row's company decides ─────────────────

        [Fact]
        public async Task An_update_is_checked_against_the_stored_rows_company_not_the_requested_one()
        {
            using var h = new InformationWriteHarness();
            h.Access.InformationLevels[Own] = 2;

            var attempts = new (string Name, Func<Task> Run)[]
            {
                ("UpdateEmail", () => h.Contacts.UpdateEmailAsync(h.StoredEmail(Other), new CompanyEmailViewDto { CompanyId = Own, EmailAddress = "x@example.test" })),
                ("UpdatePhone", () => h.Contacts.UpdatePhoneAsync(h.StoredPhone(Other), new CompanyPhoneViewDto { CompanyId = Own, PhoneNumber = "1" })),
                ("UpdateAddress", () => h.Contacts.UpdateAddressAsync(h.StoredAddress(Other), new CompanyAddressViewDto { CompanyId = Own, PostalCode = "1" })),
                ("UpdateWebsite", () => h.Contacts.UpdateWebsiteAsync(h.StoredWebsite(Other), new CompanyWebsiteViewDto { CompanyId = Own, Url = "https://x.example.test" })),
                ("UpsertTranslations", () => h.NameManagement.UpsertTranslationsAsync(new UpsertNameTranslationsDto
                {
                    NameHistoryId = h.StoredNameHistory(Other),
                    CompanyId = Own,
                    Translations = new List<NameHistoryTranslationDto> { new() { LanguageId = 12, Name = "x" } },
                })),
            };

            foreach (var (name, run) in attempts)
            {
                var error = await Record.ExceptionAsync(run);
                Assert.True(IsModifyDenial(error), name + " was not refused for a row of another company.");
            }

            Assert.Equal(0, h.Sentinel.Attempts);
            Assert.Empty(h.Names.Writes);
            Assert.All(h.Access.LevelQueries, q => Assert.Equal(Other, q.CompanyId));
        }

        [Fact]
        public void Deleting_another_companys_name_entry_under_a_permitted_company_is_refused()
        {
            using var h = new InformationWriteHarness();
            h.Access.InformationLevels[Own] = 2;

            var result = h.NameManagement.DeleteNameHistory(h.StoredNameHistory(Other), Own);

            Assert.False(result.Success);
            Assert.Empty(h.Names.Writes);
        }

        [Fact]
        public async Task A_contact_entry_cannot_be_moved_to_another_company()
        {
            using var h = new InformationWriteHarness();
            h.Access.InformationLevels[Own] = 2;
            h.Access.InformationLevels[Other] = 2;

            var attempts = new (string Name, Func<Task> Run)[]
            {
                ("UpdateEmail", () => h.Contacts.UpdateEmailAsync(h.StoredEmail(Own), new CompanyEmailViewDto { CompanyId = Other, EmailAddress = "x@example.test" })),
                ("UpdatePhone", () => h.Contacts.UpdatePhoneAsync(h.StoredPhone(Own), new CompanyPhoneViewDto { CompanyId = Other, PhoneNumber = "1" })),
                ("UpdateAddress", () => h.Contacts.UpdateAddressAsync(h.StoredAddress(Own), new CompanyAddressViewDto { CompanyId = Other, PostalCode = "1" })),
                ("UpdateWebsite", () => h.Contacts.UpdateWebsiteAsync(h.StoredWebsite(Own), new CompanyWebsiteViewDto { CompanyId = Other, Url = "https://x.example.test" })),
            };

            foreach (var (name, run) in attempts)
            {
                var error = await Record.ExceptionAsync(run);
                Assert.True(error is BusinessRuleException && error.Message.Contains("cannot be moved to another company", StringComparison.Ordinal),
                    name + " did not reject a move; outcome: " + (error?.GetType().Name ?? "completed"));
            }

            Assert.Equal(0, h.Sentinel.Attempts);
        }

        [Fact]
        public async Task Name_translations_cannot_be_written_under_another_company()
        {
            using var h = new InformationWriteHarness();
            h.Access.InformationLevels[Own] = 2;
            h.Access.InformationLevels[Other] = 2;

            var error = await Record.ExceptionAsync(() => h.NameManagement.UpsertTranslationsAsync(new UpsertNameTranslationsDto
            {
                NameHistoryId = h.StoredNameHistory(Own),
                CompanyId = Other,
                Translations = new List<NameHistoryTranslationDto> { new() { LanguageId = 12, Name = "x" } },
            }));

            Assert.IsType<BusinessRuleException>(error);
            Assert.Empty(h.Names.Writes);
        }

        // ── Fails closed without a caller ─────────────────────────────────────────

        [Theory]
        [InlineData("CompanyContact.AddEmail")]
        [InlineData("CompanyContact.DeleteWebsite")]
        [InlineData("CompanySoftware.Upsert")]
        [InlineData("CompanyNameManagement.RemoveTranslation")]
        public async Task A_request_without_a_caller_is_refused(string name)
        {
            using var h = new InformationWriteHarness(withCaller: false);
            h.Access.InformationLevels[Other] = 2;

            var error = await Record.ExceptionAsync(() => Case(name).Run(h, Other));

            Assert.True(error is BusinessRuleException && error.Message.Contains("PersonId", StringComparison.Ordinal),
                name + " did not fail closed; outcome: " + (error?.GetType().Name ?? "completed"));
            Assert.Equal(0, h.Sentinel.Attempts);
            Assert.Empty(h.Names.Writes);
        }

        // ── Controls: the sentinel fires when the database is reached ────────────

        [Fact]
        public async Task Control_a_query_reaches_the_database_and_the_sentinel_fires()
        {
            using var h = new InformationWriteHarness();

            var error = await Record.ExceptionAsync(() => h.Db.Emails.ToListAsync());

            Assert.True(DatabaseSentinel.Reached(error));
            Assert.Equal(1, h.Sentinel.Attempts);
        }

        [Fact]
        public async Task Control_a_save_reaches_the_database_and_the_sentinel_fires()
        {
            using var h = new InformationWriteHarness();
            h.Db.Websites.Add(new KSS.Entity.Website { Id = Guid.NewGuid(), CompanyId = Own, Url = "https://x.example.test" });

            var error = await Record.ExceptionAsync(() => h.Db.SaveChangesAsync());

            Assert.True(DatabaseSentinel.Reached(error));
        }

        [Fact]
        public async Task Control_a_stored_row_is_found_without_a_query()
        {
            using var h = new InformationWriteHarness();
            var id = h.StoredEmail(Own);

            var row = await h.Db.Emails.FindAsync(id);

            Assert.NotNull(row);
            Assert.Equal(0, h.Sentinel.Attempts);
        }

        // ── Inventory: no company-information write escapes this file ────────────

        // Writes whose company check lives elsewhere, with where it is.
        private static readonly Dictionary<string, string> ScopedElsewhere = new()
        {
            ["CompanyDetail.Update"] = "CompanyDetailService.UpdateAsync checks Information level 2 on the company",
            ["CompanyStakeholderManagement.Add"] = "CompanyStakeholderManagementService checks the caller's level on the company",
            ["CompanyStakeholderManagement.Update"] = "CompanyStakeholderManagementService checks the caller's level on the company",
            ["CompanyStakeholderManagement.Delete"] = "CompanyStakeholderManagementService checks the caller's level on the company",
            ["CompanyDocument.Create"] = "CompanyDocumentService checks Information level 2 on the target company",
            ["CompanyDocument.ById"] = "CompanyDocumentService checks Information level 2 on the stored row's company",
            ["CompanyOperation.Insert"] = "creates a new company; there is no existing company to change",
        };

        [Fact]
        public void Every_company_information_write_outside_the_base_controller_is_covered()
        {
            var modify = HasPermissionAttribute.PolicyPrefix + "Company.Information.Modify";
            var found = typeof(CompanyContactController).Assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.GetCustomAttributes(inherit: true).OfType<HttpMethodAttribute>()
                        .Any(a => !a.HttpMethods.SequenceEqual(new[] { "GET" })))
                    .Where(m => m.GetCustomAttributes(inherit: true).Concat(t.GetCustomAttributes(inherit: true))
                        .OfType<HasPermissionAttribute>().Any(a => a.Policy == modify))
                    .Select(m => t.Name.Replace("Controller", string.Empty) + "." + m.Name))
                .ToHashSet();

            var covered = Cases.Select(c => c.Name).Concat(ScopedElsewhere.Keys).ToHashSet();

            var uncovered = found.Except(covered).OrderBy(x => x).ToList();
            var stale = covered.Except(found).OrderBy(x => x).ToList();
            Assert.True(uncovered.Count == 0, "Company-information writes with no company check on record:\n" + string.Join("\n", uncovered));
            Assert.True(stale.Count == 0, "Listed writes that no longer exist:\n" + string.Join("\n", stale));
            Assert.Equal(18, Cases.Length);
        }
    }
}
