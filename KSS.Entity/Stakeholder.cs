using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("Stakeholder", Schema = "dbo")]
    public class Stakeholder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } // شناسه
        public Guid CompanyId { get; set; } // شناسه شرکت
        public byte RelatedPartyType { get; set; } // نوع طرف مرتبط (۱=شرکت، ۲=شخص)
        public Guid RelatedPartyId { get; set; } // شناسه طرف مرتبط
        public byte StakeholderTypeId { get; set; } // شناسه نوع ذینفع
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } // تاریخ ایجاد
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; } // تاریخ به‌روزرسانی
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;
        [ForeignKey(nameof(StakeholderTypeId))]
        public StakeholderType StakeholderType { get; set; } = null!;

        public ICollection<StakeholderHistory> Histories { get; set; } = new List<StakeholderHistory>();
    }
}
