using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("Translation", Schema = "dbo")]
    public class Translation
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

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;
    }
}
