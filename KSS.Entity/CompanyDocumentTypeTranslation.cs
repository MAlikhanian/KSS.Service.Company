using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    // fa/en names for a CompanyDocumentType. Composite PK (CompanyDocumentTypeId,
    // LanguageId). LanguageId references the Common service's Language table
    // (fa = 12, en = 10) — no cross-service FK.
    [Table("CompanyDocumentTypeTranslation", Schema = "dbo")]
    public class CompanyDocumentTypeTranslation
    {
        public int CompanyDocumentTypeId { get; set; }
        public short LanguageId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(CompanyDocumentTypeId))]
        public CompanyDocumentType CompanyDocumentType { get; set; } = null!;
    }
}
