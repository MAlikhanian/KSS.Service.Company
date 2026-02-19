using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("CompanyNameHistory", Schema = "dbo")]
    public class CompanyNameHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } // شناسه
        public Guid CompanyId { get; set; } // شناسه شرکت
        public DateTime StartDate { get; set; } // تاریخ شروع
        public DateTime? EndDate { get; set; } // تاریخ پایان (NULL = جاری)
        public DateTime CreatedAt { get; set; } // تاریخ ایجاد
        public DateTime UpdatedAt { get; set; } // تاریخ به‌روزرسانی

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;

        public ICollection<CompanyNameHistoryTranslation> Translations { get; set; } = new List<CompanyNameHistoryTranslation>();
    }
}
