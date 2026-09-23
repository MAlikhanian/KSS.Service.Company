using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("Software", Schema = "dbo")]
    public class Software
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid CompanyId { get; set; }

        public ICollection<CompanySoftware> CompanySoftwares { get; set; } = new List<CompanySoftware>();

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;
    }
}
