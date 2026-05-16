using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("RoleAccess", Schema = "dbo")]
    public class RoleAccess
    {
        [Key]
        public Guid Id { get; set; }

        // NULL = grant applies to ALL companies (e.g., SuperAdmin can see everyone).
        // Non-null = grant scoped to that specific company.
        public Guid? CompanyId { get; set; }

        // Auth role Id (Auth DB). No FK across DBs — value-only reference.
        public Guid GrantedToRoleId { get; set; }

        // FK to dbo.AccessSection.
        public byte SectionId { get; set; }

        // 0=None, 1=View, 2=Edit
        public int Level { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(SectionId))]
        public AccessSection Section { get; set; } = null!;
    }
}
