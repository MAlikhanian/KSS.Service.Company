using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace KSS.Data.DbContexts
{
    public partial class MainDbContext : DbContext
    {
        // Synthetic system Guid used when no authenticated user is available
        // (anonymous registration, background workers, seed scripts).
        private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        private readonly IHttpContextAccessor? _httpContextAccessor;

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options) { }

        public MainDbContext(DbContextOptions<MainDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<KSS.Entity.Access> Accesses => Set<KSS.Entity.Access>();
        public DbSet<KSS.Entity.AccessSection> AccessSections => Set<KSS.Entity.AccessSection>();
        public DbSet<KSS.Entity.AccessSectionTranslation> AccessSectionTranslations => Set<KSS.Entity.AccessSectionTranslation>();
        public DbSet<KSS.Entity.RoleAccess> RoleAccesses => Set<KSS.Entity.RoleAccess>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // One row per (CompanyId, GrantedToPersonId, SectionId) — matches UQ_Access in migration 001.
            modelBuilder.Entity<KSS.Entity.Access>()
                .HasIndex(a => new { a.CompanyId, a.GrantedToPersonId, a.SectionId })
                .IsUnique();

            // Composite PK for AccessSectionTranslation.
            modelBuilder.Entity<KSS.Entity.AccessSectionTranslation>()
                .HasKey(t => new { t.AccessSectionId, t.LanguageId });

            // RoleAccess uniqueness is enforced via two filtered indexes declared
            // in migration 001 (per-company + global) — EF can't express filtered
            // indexes cleanly so we rely on the DB-level constraints.

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            ApplyEntityDefaults();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyEntityDefaults();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyEntityDefaults()
        {
            var now = DateTime.UtcNow;
            var callerPersonId = ResolveCallerPersonId();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    // Auto-generate Guid Id if empty
                    var idProp = entry.Entity.GetType().GetProperty("Id");
                    if (idProp != null && idProp.PropertyType == typeof(Guid))
                    {
                        var currentId = (Guid)idProp.GetValue(entry.Entity)!;
                        if (currentId == Guid.Empty)
                            idProp.SetValue(entry.Entity, Guid.CreateVersion7());
                    }

                    var createdAtProp = entry.Entity.GetType().GetProperty("CreatedAt");
                    if (createdAtProp != null)
                        createdAtProp.SetValue(entry.Entity, now);

                    var createdByProp = entry.Entity.GetType().GetProperty("CreatedBy");
                    if (createdByProp != null && createdByProp.PropertyType == typeof(Guid))
                    {
                        var currentCreatedBy = (Guid)createdByProp.GetValue(entry.Entity)!;
                        if (currentCreatedBy == Guid.Empty)
                            createdByProp.SetValue(entry.Entity, callerPersonId ?? SystemUserId);
                    }
                    // UpdatedAt and UpdatedBy intentionally left NULL on insert —
                    // only set on actual updates so the column tells you when the row last changed.
                }

                if (entry.State == EntityState.Modified)
                {
                    var updatedAtProp = entry.Entity.GetType().GetProperty("UpdatedAt");
                    if (updatedAtProp != null)
                        updatedAtProp.SetValue(entry.Entity, now);

                    var updatedByProp = entry.Entity.GetType().GetProperty("UpdatedBy");
                    if (updatedByProp != null && updatedByProp.PropertyType == typeof(Guid?) && callerPersonId.HasValue)
                        updatedByProp.SetValue(entry.Entity, (Guid?)callerPersonId.Value);
                }
            }
        }

        private Guid? ResolveCallerPersonId()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return null;
            var raw = user.FindFirstValue("personId") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }
}
