using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KSS.Entity
{
    // Lookup of company document types (e.g. Articles of Association, Official
    // Gazette). Int identity PK owned by the database — the backend never sets
    // it. Names come from CompanyDocumentTypeTranslation.
    [Table("CompanyDocumentType", Schema = "dbo")]
    public class CompanyDocumentType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        [Unicode(false)]
        public string Code { get; set; } = string.Empty;

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<CompanyDocumentTypeTranslation> Translations { get; set; } = new List<CompanyDocumentTypeTranslation>();
        public ICollection<CompanyDocument> Documents { get; set; } = new List<CompanyDocument>();
    }
}
