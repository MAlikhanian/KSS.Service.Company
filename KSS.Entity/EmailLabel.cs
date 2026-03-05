using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KSS.Entity
{
    [Table("EmailLabel", Schema = "dbo")]
    public class EmailLabel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Id { get; set; }
        [Required]
        [MaxLength(10)]
        [Unicode(false)]
        public string Code { get; set; } = string.Empty;

        public ICollection<EmailLabelTranslation> Translations { get; set; } = new List<EmailLabelTranslation>();
        public ICollection<Email> Emails { get; set; } = new List<Email>();
    }
}

