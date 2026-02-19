using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("CompanyType", Schema = "dbo")]
    public class CompanyType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Id { get; set; } // شناسه
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty; // کد

        public ICollection<CompanyTypeTranslation> Translations { get; set; } = new List<CompanyTypeTranslation>();
        public ICollection<Company> Companies { get; set; } = new List<Company>();
    }
}
