using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    /// <summary>
    /// Assigns a Company record to an owner Company (the app tenant). Many-to-many:
    /// a company record can be managed by several owner companies. Both ids point at
    /// Companies in this same DB; OwnerCompanyId is the tenant that "owns" CompanyId.
    /// </summary>
    [Table("CompanyOwnership", Schema = "dbo")]
    public class CompanyOwnership
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        // The owner/tenant company.
        public Guid OwnerCompanyId { get; set; }

        // The company record owned/managed by the owner company.
        public Guid CompanyId { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
