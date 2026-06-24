using KSS.Dto;
using KSS.Entity;

namespace KSS.Service.IService
{
    public interface IFinancialInfoService : IBaseService<FinancialInfo, FinancialInfoDto, FinancialInfoInsertDto, FinancialInfoDto>
    {
        /// <summary>A company's financial-info rows (report read — no access filter).</summary>
        Task<List<FinancialInfoDto>> GetByCompanyAsync(Guid companyId);
    }
}
