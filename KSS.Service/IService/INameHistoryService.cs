using KSS.Dto;
using KSS.Entity;
using KSS.Helper;

namespace KSS.Service.IService
{
    public interface INameHistoryService : IBaseService<NameHistory, NameHistoryDto, NameHistoryDto, NameHistoryDto>
    {
        /// <summary>
        /// Delete a name history entry with business rule validation.
        /// Returns Fail if: only entry, first entry, or not the newest entry.
        /// </summary>
        ServiceResult DeleteNameHistory(Guid id, Guid companyId);

        /// <summary>
        /// Add a new name history entry (entity) with business rule validation.
        /// Returns Fail if StartDate is before existing entries.
        /// </summary>
        Task<ServiceResult> AddNameHistoryAsync(NameHistory item, bool saveChanges = true);

        /// <summary>
        /// Add a new name history entry (DTO) with business rule validation.
        /// Returns Fail if StartDate is before existing entries.
        /// </summary>
        Task<ServiceResult> AddNameHistoryDtoAsync(NameHistoryDto item, bool saveChanges = true);
    }
}
