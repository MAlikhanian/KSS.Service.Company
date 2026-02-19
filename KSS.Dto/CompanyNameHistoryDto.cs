namespace KSS.Dto
{
    public class CompanyNameHistoryDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
