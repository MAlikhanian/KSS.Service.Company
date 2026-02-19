using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("Phone", Schema = "dbo")]
    public class Phone
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public short CountryId { get; set; } // Phone number's country code context (separate from Company.RegistrationCountryId)
        [Required]
        [MaxLength(16)]
        [Column(TypeName = "VARCHAR(16)")]
        public string PhoneNumber { get; set; } = string.Empty; // E.164: + and 7-15 digits
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;
        [ForeignKey(nameof(LabelId))]
        public PhoneLabel Label { get; set; } = null!;
    }
}

