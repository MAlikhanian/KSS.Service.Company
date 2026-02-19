using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("CompanyTypeTranslation", Schema = "dbo")]
    public class CompanyTypeTranslation
    {
        public byte CompanyTypeId { get; set; } // شناسه نوع شرکت
        public short LanguageId { get; set; } // شناسه زبان
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // نام

        [ForeignKey(nameof(CompanyTypeId))]
        public CompanyType CompanyType { get; set; } = null!;
    }
}
