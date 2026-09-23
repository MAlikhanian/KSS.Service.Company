using System.Security.Claims;
using KSS.Dto;
using KSS.Entity;
using KSS.Helper;
using KSS.Service.IService;
using KSS.Service.Service;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace KSS.Api.Tests
{
    /// <summary>
    /// Company-scoped authorization for document rows. The controller's permission
    /// attribute is global to the caller; these tests pin the per-company check that
    /// the service applies on top of it.
    /// </summary>
    public class CompanyDocumentServiceTests
    {
        private static readonly Guid CompanyA = Guid.Parse("00000000-0000-0000-0000-00000000000a");
        private static readonly Guid CompanyB = Guid.Parse("00000000-0000-0000-0000-00000000000b");
        private static readonly Guid Caller = Guid.Parse("00000000-0000-0000-0000-0000000000c1");
        private static readonly Guid RowInB = Guid.Parse("00000000-0000-0000-0000-0000000000d1");
        private static readonly Guid RowInA = Guid.Parse("00000000-0000-0000-0000-0000000000d2");

        private sealed record Harness(
            CompanyDocumentService Service,
            FakeDocumentRepository Repository,
            FakeAccessService Access);

        private static Harness Build(bool withCaller = true)
        {
            var (repository, repositoryState) = FakeDocumentRepository.Create();
            repositoryState.Rows.Add(new CompanyDocument
            {
                Id = RowInB,
                CompanyId = CompanyB,
                CompanyDocumentTypeId = 1,
                FileName = "original.pdf",
            });
            repositoryState.Rows.Add(new CompanyDocument
            {
                Id = RowInA,
                CompanyId = CompanyA,
                CompanyDocumentTypeId = 1,
                FileName = "other.pdf",
            });

            var (access, accessState) = FakeAccessService.Create();

            var user = withCaller
                ? new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("personId", Caller.ToString()) }, "Test"))
                : new ClaimsPrincipal(new ClaimsIdentity());
            var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = user } };

            var service = new CompanyDocumentService(FakeMapper.Create(), repository, access, accessor);
            return new Harness(service, repositoryState, accessState);
        }

        // ── Read ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Listing_is_denied_without_read_level_on_that_company()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 0;

            await Assert.ThrowsAsync<BusinessRuleException>(() => h.Service.GetByCompanyAsync(CompanyB));
            Assert.DoesNotContain("ToListAsync", h.Repository.Calls);
        }

        [Fact]
        public async Task Listing_is_allowed_with_read_level_on_that_company()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 1;

            await h.Service.GetByCompanyAsync(CompanyB);

            Assert.Contains("ToListAsync", h.Repository.Calls);
            Assert.Equal((CompanyB, Caller), Assert.Single(h.Access.LevelQueries));
        }

        [Fact]
        public async Task Read_level_on_another_company_does_not_open_this_one()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyA] = 2;
            h.Access.InformationLevels[CompanyB] = 0;

            await Assert.ThrowsAsync<BusinessRuleException>(() => h.Service.GetByCompanyAsync(CompanyB));
        }

        // ── Create ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_is_denied_below_modify_level()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 1;

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => h.Service.CreateAsync(new CompanyDocumentInsertDto { CompanyId = CompanyB, FileName = "new.pdf" }));
            Assert.Empty(h.Repository.Added);
        }

        [Fact]
        public async Task Create_is_allowed_with_modify_level_on_the_target_company()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 2;

            await h.Service.CreateAsync(new CompanyDocumentInsertDto { CompanyId = CompanyB, FileName = "new.pdf" });

            var added = Assert.Single(h.Repository.Added);
            Assert.Equal(CompanyB, added.CompanyId);
        }

        // ── Delete ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_is_authorised_against_the_stored_rows_company()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyA] = 2;   // modify rights elsewhere
            h.Access.InformationLevels[CompanyB] = 0;   // none on the row's company

            await Assert.ThrowsAsync<BusinessRuleException>(() => h.Service.DeleteByIdAsync(RowInB));
            Assert.Empty(h.Repository.Removed);
            Assert.Equal(CompanyB, Assert.Single(h.Access.LevelQueries).CompanyId);
        }

        [Fact]
        public async Task Delete_is_allowed_with_modify_level_on_the_rows_company()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 2;

            await h.Service.DeleteByIdAsync(RowInB);

            Assert.Equal(RowInB, Assert.Single(h.Repository.Removed).Id);
        }

        [Fact]
        public async Task Delete_of_a_missing_row_is_a_no_op()
        {
            var h = Build();

            await h.Service.DeleteByIdAsync(Guid.NewGuid());

            Assert.Empty(h.Repository.Removed);
            Assert.Empty(h.Access.LevelQueries);
        }

        // ── Update ────────────────────────────────────────────────────────────────
        // Called through the service interface, exactly as BaseController does. On the
        // concrete type a one-argument call binds to the mapping overload instead, which
        // never reaches the persisting override.

        private static ICompanyDocumentService AsControllerSeesIt(Harness h) => h.Service;

        [Fact]
        public void Update_is_denied_below_modify_level_on_the_rows_company()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 1;

            Assert.Throws<BusinessRuleException>(
                () => AsControllerSeesIt(h).UpdateDto(new CompanyDocumentUpdateDto { Id = RowInB, CompanyDocumentTypeId = 2 }));
            Assert.Empty(h.Repository.Updated);
        }

        [Fact]
        public void Update_keeps_the_row_in_its_own_company()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 2;

            AsControllerSeesIt(h).UpdateDto(new CompanyDocumentUpdateDto { Id = RowInB, CompanyDocumentTypeId = 2, FileName = "renamed.pdf" });

            var saved = Assert.Single(h.Repository.Updated);
            Assert.Equal(CompanyB, saved.CompanyId);
            Assert.Equal(2, saved.CompanyDocumentTypeId);
            Assert.Equal("renamed.pdf", saved.FileName);
        }

        [Fact]
        public void Update_contract_cannot_express_a_company_change()
        {
            Assert.Null(typeof(CompanyDocumentUpdateDto).GetProperty("CompanyId"));
        }

        // ── Inherited CRUD writes (called through the interface, as BaseController does) ──

        private CompanyDocument Stored(Harness h, Guid id) => h.Repository.Rows.Single(r => r.Id == id);

        [Fact]
        public async Task Inherited_add_is_denied_below_modify_level()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 1;

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => AsControllerSeesIt(h).AddAsync(new CompanyDocument { CompanyId = CompanyB }));
            Assert.Empty(h.Repository.Added);
        }

        [Fact]
        public async Task Inherited_add_is_allowed_with_modify_level()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 2;

            await AsControllerSeesIt(h).AddAsync(new CompanyDocument { CompanyId = CompanyB });

            Assert.Equal(CompanyB, Assert.Single(h.Repository.Added).CompanyId);
        }

        [Fact]
        public async Task Inherited_add_from_dto_is_denied_below_modify_level()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 1;

            await Assert.ThrowsAsync<BusinessRuleException>(
                () => AsControllerSeesIt(h).AddDtoAsync(new CompanyDocumentInsertDto { CompanyId = CompanyB }));
            Assert.Empty(h.Repository.Added);
        }

        [Fact]
        public async Task Inherited_add_from_dto_is_allowed_with_modify_level()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 2;

            await AsControllerSeesIt(h).AddDtoAsync(new CompanyDocumentInsertDto { CompanyId = CompanyB });

            Assert.Equal(CompanyB, Assert.Single(h.Repository.Added).CompanyId);
        }

        [Fact]
        public async Task Add_range_is_all_or_nothing()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyA] = 2;
            h.Access.InformationLevels[CompanyB] = 0;

            await Assert.ThrowsAsync<BusinessRuleException>(() => AsControllerSeesIt(h).AddRangeAsync(new[]
            {
                new CompanyDocument { CompanyId = CompanyA },
                new CompanyDocument { CompanyId = CompanyB },
            }));
            Assert.Empty(h.Repository.Added);
        }

        [Fact]
        public void Inherited_update_is_checked_against_the_stored_row()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 1;

            Assert.Throws<BusinessRuleException>(() => AsControllerSeesIt(h).Update(
                new CompanyDocument { Id = RowInB, CompanyId = CompanyB, FileName = "changed.pdf" }));
            Assert.Empty(h.Repository.Updated);
            Assert.Equal("original.pdf", Stored(h, RowInB).FileName);
        }

        [Fact]
        public void Inherited_update_cannot_move_a_row_to_another_company()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyA] = 2;
            h.Access.InformationLevels[CompanyB] = 2;

            Assert.Throws<BusinessRuleException>(() => AsControllerSeesIt(h).Update(
                new CompanyDocument { Id = RowInB, CompanyId = CompanyA, FileName = "moved.pdf" }));
            Assert.Empty(h.Repository.Updated);
            Assert.Equal(CompanyB, Stored(h, RowInB).CompanyId);
            Assert.Equal("original.pdf", Stored(h, RowInB).FileName);
        }

        [Fact]
        public void Inherited_update_writes_through_the_stored_row()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 2;

            AsControllerSeesIt(h).Update(new CompanyDocument { Id = RowInB, CompanyId = CompanyB, FileName = "changed.pdf" });

            var saved = Assert.Single(h.Repository.Updated);
            Assert.Same(Stored(h, RowInB), saved);
            Assert.Equal("changed.pdf", saved.FileName);
        }

        [Fact]
        public void Update_range_is_all_or_nothing()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyA] = 0;
            h.Access.InformationLevels[CompanyB] = 2;

            Assert.Throws<BusinessRuleException>(() => AsControllerSeesIt(h).UpdateRange(new[]
            {
                new CompanyDocument { Id = RowInB, CompanyId = CompanyB, FileName = "b.pdf" },
                new CompanyDocument { Id = RowInA, CompanyId = CompanyA, FileName = "a.pdf" },
            }));
            Assert.Empty(h.Repository.Updated);
            Assert.Equal("original.pdf", Stored(h, RowInB).FileName);
        }

        [Fact]
        public void Inherited_remove_is_checked_against_the_stored_row_not_the_body()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyA] = 2;   // the company the body claims
            h.Access.InformationLevels[CompanyB] = 0;   // the company the row belongs to

            Assert.Throws<BusinessRuleException>(
                () => AsControllerSeesIt(h).Remove(new CompanyDocument { Id = RowInB, CompanyId = CompanyA }));
            Assert.Empty(h.Repository.Removed);
        }

        [Fact]
        public void Inherited_remove_removes_the_stored_row()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyB] = 2;

            AsControllerSeesIt(h).Remove(new CompanyDocument { Id = RowInB, CompanyId = CompanyB });

            Assert.Same(Stored(h, RowInB), Assert.Single(h.Repository.Removed));
        }

        [Fact]
        public void Remove_range_is_all_or_nothing()
        {
            var h = Build();
            h.Access.InformationLevels[CompanyA] = 0;
            h.Access.InformationLevels[CompanyB] = 2;

            Assert.Throws<BusinessRuleException>(() => AsControllerSeesIt(h).RemoveRange(new[]
            {
                new CompanyDocument { Id = RowInB, CompanyId = CompanyB },
                new CompanyDocument { Id = RowInA, CompanyId = CompanyA },
            }));
            Assert.Empty(h.Repository.Removed);
        }

        // ── Caller ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task A_request_without_a_caller_is_denied_before_any_data_access()
        {
            var h = Build(withCaller: false);
            h.Access.InformationLevels[CompanyB] = 2;

            await Assert.ThrowsAsync<BusinessRuleException>(() => h.Service.GetByCompanyAsync(CompanyB));
            Assert.Empty(h.Repository.Calls);
            Assert.Empty(h.Access.LevelQueries);
        }
    }
}
