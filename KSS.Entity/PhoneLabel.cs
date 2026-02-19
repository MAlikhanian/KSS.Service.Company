using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("PhoneLabel", Schema = "dbo")]
    public class PhoneLabel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Id { get; set; }
        [Required]
        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        public ICollection<PhoneLabelTranslation> Translations { get; set; } = new List<PhoneLabelTranslation>();
        public ICollection<Phone> Phones { get; set; } = new List<Phone>();
    }
}

