namespace KSS.Dto
{
    /// <summary>
    /// Returned to a caller asking "what are my levels for this company?".
    /// Each value is 0=None, 1=View, 2=Edit. Owners (Company.CreatedBy == caller)
    /// always see {2, 2}.
    /// </summary>
    public class AccessLevelsDto
    {
        public int Information { get; set; }
        public int Access      { get; set; }
    }
}
