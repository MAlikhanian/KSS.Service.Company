using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("NameHistoryTranslation", Schema = "dbo")]
    public class NameHistoryTranslation
    {
        public Guid NameHistoryId { get; set; } // شناسه تاریخچه نام شرکت
        public short LanguageId { get; set; } // شناسه زبان
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty; // نام
        [MaxLength(50)]
        public string? ShortName { get; set; } // نام کوتاه

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(NameHistoryId))]
        public NameHistory NameHistory { get; set; } = null!;
    }
}
