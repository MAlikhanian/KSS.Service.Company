using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("PhoneLabelTranslation", Schema = "dbo")]
    public class PhoneLabelTranslation
    {
        public byte PhoneLabelId { get; set; }
        public short LanguageId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [ForeignKey(nameof(PhoneLabelId))]
        public PhoneLabel PhoneLabel { get; set; } = null!;
    }
}

