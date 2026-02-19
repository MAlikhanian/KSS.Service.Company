using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("CompanyNameHistoryTranslation", Schema = "dbo")]
    public class CompanyNameHistoryTranslation
    {
        public Guid CompanyNameHistoryId { get; set; } // شناسه تاریخچه نام شرکت
        public short LanguageId { get; set; } // شناسه زبان
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty; // نام
        [MaxLength(50)]
        public string? ShortName { get; set; } // نام کوتاه

        [ForeignKey(nameof(CompanyNameHistoryId))]
        public CompanyNameHistory NameHistory { get; set; } = null!;
    }
}
