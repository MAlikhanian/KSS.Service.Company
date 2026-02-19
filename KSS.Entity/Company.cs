using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("Company", Schema = "dbo")]
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public byte CompanyTypeId { get; set; }
        public short? IndustryId { get; set; } // Industry. NULL = unknown at creation (often set after verification/onboarding). If your domain always requires industry: make NOT NULL; or add Industry.Code='Unknown' and default to it.
        public DateTime RegistrationDate { get; set; } // تاریخ ثبت
        [Required]
        [MaxLength(30)]
        [Column(TypeName = "VARCHAR(30)")]
        public string RegistrationNo { get; set; } = string.Empty; // شماره ثبت
        [Required]
        [MaxLength(20)]
        [Column(TypeName = "VARCHAR(20)")]
        public string NationalId { get; set; } = string.Empty; // شناسه ملی
        [Required]
        [MaxLength(20)]
        [Column(TypeName = "VARCHAR(20)")]
        public string EconomicCode { get; set; } = string.Empty; // کد اقتصادی
        public short RegistrationCountryId { get; set; } // کشور (ref Common)
        public short RegistrationRegionId { get; set; } // استان / منطقه
        public int RegistrationCityId { get; set; } // شهر
        [MaxLength(30)]
        [Column(TypeName = "VARCHAR(30)")]
        public string? TaxId { get; set; } // شناسه مالیاتی
        public DateTime? FoundedDate { get; set; } // تاریخ تأسیس
        [MaxLength(256)]
        [Column(TypeName = "VARCHAR(256)")]
        public string? Website { get; set; } // وب‌سایت
        [MaxLength(512)]
        [Column(TypeName = "VARCHAR(512)")]
        public string? LogoUrl { get; set; } // آدرس لوگو
        public bool IsActive { get; set; } = true; // فعال
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(CompanyTypeId))]
        public CompanyType CompanyType { get; set; } = null!;
        [ForeignKey(nameof(IndustryId))]
        public Industry? Industry { get; set; }

        public ICollection<CompanyTranslation> Translations { get; set; } = new List<CompanyTranslation>();
        public ICollection<CompanyNameHistory> NameHistories { get; set; } = new List<CompanyNameHistory>();
        public ICollection<CompanyStakeholder> Stakeholders { get; set; } = new List<CompanyStakeholder>();
        public ICollection<Email> Emails { get; set; } = new List<Email>();
        public ICollection<Phone> Phones { get; set; } = new List<Phone>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
