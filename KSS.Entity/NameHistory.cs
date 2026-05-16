using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("NameHistory", Schema = "dbo")]
    public class NameHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } // شناسه
        public Guid CompanyId { get; set; } // شناسه شرکت
        [Column(TypeName = "DATE")]
        public DateTime StartDate { get; set; } // تاریخ شروع
        [Column(TypeName = "DATE")]
        public DateTime? EndDate { get; set; } // تاریخ پایان (NULL = جاری)
        [MaxLength(500)]
        public string? Description { get; set; } // توضیحات تغییر نام
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } // تاریخ ایجاد
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; } // تاریخ به‌روزرسانی
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;

        public ICollection<NameHistoryTranslation> Translations { get; set; } = new List<NameHistoryTranslation>();
    }
}
