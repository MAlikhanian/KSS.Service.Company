using KSS.Dto;

namespace KSS.Service.IService
{
    public interface IBrokerageSelectService
    {
        Task<IEnumerable<BrokerageSelectDto>> GetBrokerageSelectListAsync(short languageId, string? query = null);
    }
}
