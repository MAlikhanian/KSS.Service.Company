namespace KSS.Service.IService
{
    /// <summary>
    /// Declares estate-wide reference data (labels, types, categories, industries, legal forms,
    /// their translations, and the software catalog): rows that are no company's own records,
    /// so the generic reads return them to any caller holding the read permission.
    ///
    /// Only for such tables. A table holding a company's own records must scope its reads
    /// (ICompanyScopedReads) instead; declaring it here would let every holder read every
    /// company's rows. This declaration opens reads only: the generic writes still need
    /// ICompanyScopedWrites.
    /// </summary>
    public interface IReferenceData
    {
    }
}
