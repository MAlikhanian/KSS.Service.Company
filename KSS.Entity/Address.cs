using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("Address", Schema = "dbo")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        public short CountryId { get; set; }
        public short RegionId { get; set; }
        public int CityId { get; set; }
        [Required]
        [MaxLength(20)]
        [Column(TypeName = "VARCHAR(20)")]
        public string PostalCode { get; set; } = string.Empty; // Multi-country; some codes exceed 10 or include letters/spaces
        [Column(TypeName = "decimal(9, 6)")]
        public decimal? Latitude { get; set; }
        [Column(TypeName = "decimal(9, 6)")]
        public decimal? Longitude { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;
        [ForeignKey(nameof(LabelId))]
        public AddressLabel Label { get; set; } = null!;
        public ICollection<AddressTranslation> Translations { get; set; } = new List<AddressTranslation>();
    }
}

