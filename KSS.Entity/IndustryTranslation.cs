using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("IndustryTranslation", Schema = "dbo")]
    public class IndustryTranslation
    {
        public short IndustryId { get; set; } // شناسه صنعت
        public short LanguageId { get; set; } // شناسه زبان
        [Required]
        [MaxLength(80)]
        public string Name { get; set; } = string.Empty; // نام

        [ForeignKey(nameof(IndustryId))]
        public Industry Industry { get; set; } = null!;
    }
}
