using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KSS.Entity
{
    [Table("Industry", Schema = "dbo")]
    public class Industry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short Id { get; set; } // شناسه
        [Required]
        [MaxLength(20)]
        [Unicode(false)]
        public string Code { get; set; } = string.Empty; // کد

        public ICollection<IndustryTranslation> Translations { get; set; } = new List<IndustryTranslation>();
        public ICollection<Company> Companies { get; set; } = new List<Company>();
    }
}
