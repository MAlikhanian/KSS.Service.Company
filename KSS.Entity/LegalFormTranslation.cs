using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("LegalFormTranslation", Schema = "dbo")]
    public class LegalFormTranslation
    {
        public byte LegalFormId { get; set; } // شناسه نوع شرکت
        public short LanguageId { get; set; } // شناسه زبان
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // نام

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(LegalFormId))]
        public LegalForm LegalForm { get; set; } = null!;
    }
}
