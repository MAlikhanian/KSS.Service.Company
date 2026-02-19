using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("CompanyStakeholder", Schema = "dbo")]
    public class CompanyStakeholder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } // شناسه
        public Guid CompanyId { get; set; } // شناسه شرکت
        public byte RelatedPartyType { get; set; } // نوع طرف مرتبط (۱=شرکت، ۲=شخص)
        public Guid RelatedPartyId { get; set; } // شناسه طرف مرتبط
        public byte StakeholderTypeId { get; set; } // شناسه نوع ذینفع
        public DateTime CreatedAt { get; set; } // تاریخ ایجاد
        public DateTime UpdatedAt { get; set; } // تاریخ به‌روزرسانی

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;
        [ForeignKey(nameof(StakeholderTypeId))]
        public StakeholderType StakeholderType { get; set; } = null!;

        public ICollection<CompanyStakeholderHistory> Histories { get; set; } = new List<CompanyStakeholderHistory>();
    }
}
