using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("AddressLabelTranslation", Schema = "dbo")]
    public class AddressLabelTranslation
    {
        public byte AddressLabelId { get; set; }
        public short LanguageId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [ForeignKey(nameof(AddressLabelId))]
        public AddressLabel AddressLabel { get; set; } = null!;
    }
}

