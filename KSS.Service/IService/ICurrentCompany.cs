namespace KSS.Service.IService
{
    /// <summary>
    /// The owner company selected for the current request, taken from the
    /// <c>X-Company-Id</c> header. Null when no (valid) header is present, in
    /// which case tenant filtering is skipped and legacy behaviour applies.
    /// </summary>
    public interface ICurrentCompany
    {
        Guid? CompanyId { get; }
    }
}
