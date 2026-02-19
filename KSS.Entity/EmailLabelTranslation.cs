using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("EmailLabelTranslation", Schema = "dbo")]
    public class EmailLabelTranslation
    {
        public byte EmailLabelId { get; set; }
        public short LanguageId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [ForeignKey(nameof(EmailLabelId))]
        public EmailLabel EmailLabel { get; set; } = null!;
    }
}

