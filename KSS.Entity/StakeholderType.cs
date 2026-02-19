using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("StakeholderType", Schema = "dbo")]
    public class StakeholderType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Id { get; set; } // شناسه
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty; // کد

        public ICollection<StakeholderTypeTranslation> Translations { get; set; } = new List<StakeholderTypeTranslation>();
        public ICollection<CompanyStakeholder> Stakeholders { get; set; } = new List<CompanyStakeholder>();
    }
}
