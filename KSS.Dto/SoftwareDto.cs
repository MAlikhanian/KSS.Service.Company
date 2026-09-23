namespace KSS.Dto
{
    public class SoftwareDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid CompanyId { get; set; }
    }
}
