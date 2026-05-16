using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("AddressTranslation", Schema = "dbo")]
    public class AddressTranslation
    {
        public Guid AddressId { get; set; }
        public short LanguageId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Street1 { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? Street2 { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [ForeignKey(nameof(AddressId))]
        public Address Address { get; set; } = null!;
    }
}

