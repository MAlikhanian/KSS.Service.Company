namespace KSS.Dto
{
    /// <summary>
    /// Minimal DTO returned by GET /Api/Company/Count. The dashboard "All
    /// Companies" tile fetches this — no entity rows are exposed, only the
    /// scalar count.
    /// </summary>
    public class CompanyCountDto
    {
        public int Count { get; set; }
    }
}
