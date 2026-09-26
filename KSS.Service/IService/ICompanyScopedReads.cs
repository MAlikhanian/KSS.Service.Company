namespace KSS.Service.IService
{
    /// <summary>
    /// Declares that every generic read this service exposes through BaseController is limited
    /// to the companies the caller may read: a list returns only those companies' rows, and a
    /// single row of any other company is refused.
    ///
    /// BaseController refuses its generic read actions for any service that declares neither
    /// this nor IReferenceData. That refusal is deliberate: a permission is global to the
    /// caller, so an unscoped read would show every company's records to any holder. The
    /// declaration is not a way to switch the refusal off. Add it only together with that
    /// scoping, and with tests proving it; a test fails if a declared service returns another
    /// company's row.
    /// </summary>
    public interface ICompanyScopedReads
    {
    }
}
