namespace KSS.Dto
{
    // No Id — the backend stamps a v7 GUID (ApplyEntityDefaults); a client GUID cannot bind.
    public class FinancialInfoInsertDto
    {
        public Guid CompanyId { get; set; }
        public short FiscalYear { get; set; }
        public decimal RegisteredCapital { get; set; }
        public long NumberOfShares { get; set; }
    }
}
