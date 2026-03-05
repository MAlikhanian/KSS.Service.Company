using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KSS.Entity
{
    [Table("AddressLabel", Schema = "dbo")]
    public class AddressLabel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Id { get; set; }
        [Required]
        [MaxLength(10)]
        [Unicode(false)]
        public string Code { get; set; } = string.Empty;

        public ICollection<AddressLabelTranslation> Translations { get; set; } = new List<AddressLabelTranslation>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}

