namespace KSS.Service.IService
{
    /// <summary>
    /// Declares that every write this service exposes through BaseController checks the
    /// caller's company level: on the TARGET company for an add, and on the STORED row's
    /// company for an update or removal, refusing a change of company.
    ///
    /// BaseController refuses its generic write actions for any service that does not carry
    /// this declaration. That refusal is deliberate: a permission is global to the caller,
    /// so a write without a company check lets a holder change any company's records. The
    /// declaration is not a way to switch the refusal off. Add it only together with those
    /// checks, and with tests proving them; a test fails if a declared service writes for a
    /// caller who lacks the level on the company.
    /// </summary>
    public interface ICompanyScopedWrites
    {
    }
}
