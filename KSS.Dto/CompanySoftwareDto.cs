namespace KSS.Dto
{
    public class CompanySoftwareDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public byte SoftwareCategoryId { get; set; }
        public int SoftwareId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
