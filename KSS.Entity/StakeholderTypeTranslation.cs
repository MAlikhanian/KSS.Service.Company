using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("StakeholderTypeTranslation", Schema = "dbo")]
    public class StakeholderTypeTranslation
    {
        public byte StakeholderTypeId { get; set; } // شناسه نوع ذینفع
        public short LanguageId { get; set; } // شناسه زبان
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // نام

        [ForeignKey(nameof(StakeholderTypeId))]
        public StakeholderType StakeholderType { get; set; } = null!;
    }
}
