using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("StakeholderHistory", Schema = "dbo")]
    public class StakeholderHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; } // شناسه
        public Guid CompanyStakeholderId { get; set; } // شناسه ذینفع شرکت
        [Column(TypeName = "decimal(5, 2)")]
        public decimal OwnershipPercentage { get; set; } // درصد مالکیت
        public long ShareCount { get; set; } // تعداد سهام
        public Guid? BoardRepresentativePersonId { get; set; } // شناسه نماینده هیئت مدیره (اختیاری)
        [Column(TypeName = "DATE")]
        public DateTime RegistrationDate { get; set; } // تاریخ ثبت (official legal filing date)
        [Column(TypeName = "DATE")]
        public DateTime EffectiveDate { get; set; } // تاریخ اعمال تغییرات (when change takes effect; may differ from RegistrationDate)
        [Column(TypeName = "DATE")]
        public DateTime? EndDate { get; set; } // تاریخ پایان اعتبار (NULL = جاری)
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } // تاریخ ایجاد
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; } // تاریخ به‌روزرسانی
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(CompanyStakeholderId))]
        public Stakeholder Stakeholder { get; set; } = null!;
    }
}
