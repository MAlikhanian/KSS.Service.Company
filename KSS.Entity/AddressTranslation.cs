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

        [ForeignKey(nameof(AddressId))]
        public Address Address { get; set; } = null!;
    }
}

