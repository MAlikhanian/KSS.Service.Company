using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("Email", Schema = "dbo")]
    public class Email
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte LabelId { get; set; }
        [Required]
        [MaxLength(128)]
        [Column("Email", TypeName = "VARCHAR(128)")]
        public string EmailAddress { get; set; } = string.Empty; // Normalized: trimmed, lowercase
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;
        [ForeignKey(nameof(LabelId))]
        public EmailLabel Label { get; set; } = null!;
    }
}

