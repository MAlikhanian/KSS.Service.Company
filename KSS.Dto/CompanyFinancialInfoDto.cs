namespace KSS.Dto
{
    public class CompanyFinancialInfoDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public short FiscalYear { get; set; }
        public decimal RegisteredCapital { get; set; }
        public long NumberOfShares { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
