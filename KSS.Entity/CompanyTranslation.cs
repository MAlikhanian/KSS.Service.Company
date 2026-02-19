using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("CompanyTranslation", Schema = "dbo")]
    public class CompanyTranslation
    {
        public Guid CompanyId { get; set; } // شناسه شرکت
        public short LanguageId { get; set; } // شناسه زبان
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty; // نام
        [MaxLength(50)]
        public string? ShortName { get; set; } // نام کوتاه
        [MaxLength(500)]
        public string? Description { get; set; } // توضیحات

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;
    }
}
