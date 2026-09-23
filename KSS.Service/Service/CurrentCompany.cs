using KSS.Service.IService;
using Microsoft.AspNetCore.Http;

namespace KSS.Service.Service
{
    public class CurrentCompany : ICurrentCompany
    {
        public Guid? CompanyId { get; }

        public CurrentCompany(IHttpContextAccessor httpContextAccessor)
        {
            var ctx = httpContextAccessor?.HttpContext;
            string? header = null;
            if (ctx != null && ctx.Request.Headers.TryGetValue("X-Company-Id", out var value))
                header = value.ToString();

            CompanyId = Guid.TryParse(header, out var id) && id != Guid.Empty ? id : (Guid?)null;
        }
    }
}
