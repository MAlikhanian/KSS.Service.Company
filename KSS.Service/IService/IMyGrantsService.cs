using KSS.Dto;

namespace KSS.Service.IService
{
    public interface IMyGrantsService
    {
        /// <summary>
        /// The caller's own grants: every personal grant of the caller and every grant of the
        /// caller's roles, live or not, with the companies they name. The caller and the roles are
        /// the signed-in identity's; this is never called with anyone else's.
        /// </summary>
        Task<MyGrantsDto> GetAsync(Guid callerPersonId, IReadOnlyCollection<Guid> callerRoleIds);
    }
}
