using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSS.Entity
{
    [Table("Access", Schema = "dbo")]
    public class Access
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }          // company being granted access to
        public Guid GrantedToPersonId { get; set; }  // person who has access

        // FK to dbo.AccessSection. See AccessSectionId for the constant byte values.
        public byte SectionId { get; set; }

        // 0=None, 1=View, 2=Edit
        public int Level { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(SectionId))]
        public AccessSection Section { get; set; } = null!;
    }
}
