using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KSS.Entity
{
    // A single uploaded company document / image. Stores METADATA only — the
    // file bytes live in a FileStorage instance (category 'CompanyDocument'),
    // brokered by the FileOrchestrator. StorageInstanceId points at the
    // orchestrator's StorageInstance row that holds this file's bytes.
    // Mirrors KSS.Service.Person.Document.
    [Table("CompanyDocument", Schema = "dbo")]
    public class CompanyDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public int CompanyDocumentTypeId { get; set; }
        public int StorageInstanceId { get; set; }
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        [Required]
        [MaxLength(100)]
        [Unicode(false)]
        public string ContentType { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; } = null!;
        [ForeignKey(nameof(CompanyDocumentTypeId))]
        public CompanyDocumentType CompanyDocumentType { get; set; } = null!;
    }
}
