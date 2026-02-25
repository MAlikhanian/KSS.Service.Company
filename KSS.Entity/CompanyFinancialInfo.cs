using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("CompanyFinancialInfo", Schema = "dbo")]
    public class CompanyFinancialInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public short FiscalYear { get; set; }
        [Column(TypeName = "DECIMAL(18,0)")]
        public decimal RegisteredCapital { get; set; }
        public long NumberOfShares { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;
    }
}
