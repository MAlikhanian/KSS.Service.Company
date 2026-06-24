using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace KSS.Service.Client
{
    /// <summary>
    /// Forwards the incoming request's Bearer token to the upstream Person API
    /// so the original caller's identity is preserved — no service-token /
    /// impersonation layer needed.
    /// </summary>
    public class PersonForwardAuthHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PersonForwardAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var incoming = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(incoming) && AuthenticationHeaderValue.TryParse(incoming, out var parsed))
            {
                request.Headers.Authorization = parsed;
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
