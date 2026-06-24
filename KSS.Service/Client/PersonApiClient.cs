using System.Net.Http.Json;
using KSS.Dto;
using Microsoft.Extensions.Logging;

namespace KSS.Service.Client
{
    /// <summary>
    /// Typed HTTP client for KSS.Service.Person. The caller's Bearer token is
    /// forwarded by <see cref="PersonForwardAuthHandler"/> (registered on the
    /// HttpClient), so the Person service's auth runs against the real caller.
    /// </summary>
    public class PersonApiClient : IPersonApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<PersonApiClient> _logger;

        public PersonApiClient(HttpClient http, ILogger<PersonApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<IReadOnlyDictionary<Guid, string>> GetPersonNamesAsync(
            IReadOnlyCollection<Guid> personIds,
            short languageId = 12,
            CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<Guid, string>();
            if (personIds == null || personIds.Count == 0) return result;

            try
            {
                var request = new PersonNamesRequestDto
                {
                    Ids = personIds.Distinct().ToList(),
                    LanguageId = languageId,
                };

                // Use JsonContent.Create + PostAsync (not PostAsJsonAsync) to avoid an
                // ambiguity with the legacy System.Net.Http.Formatting extension that is
                // also referenced in this solution.
                using var response = await _http.PostAsync(
                    "Api/Person/Names",
                    JsonContent.Create(request),
                    cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Person/Names returned {Status}; stakeholder names left unresolved.",
                        (int)response.StatusCode);
                    return result;
                }

                var names = await response.Content.ReadFromJsonAsync<List<PersonNameDto>>(cancellationToken: cancellationToken)
                            ?? new List<PersonNameDto>();

                foreach (var n in names)
                {
                    var full = $"{n.FirstName} {n.LastName}".Trim();
                    result[n.Id] = string.IsNullOrWhiteSpace(full) ? (n.NationalId ?? string.Empty) : full;
                }
            }
            catch (Exception ex)
            {
                // Graceful degradation: never let a Person-service problem turn
                // the stakeholder list into a 500. Names simply stay unresolved.
                _logger.LogWarning(ex, "Failed to resolve person names from Person service; leaving unresolved.");
            }

            return result;
        }
    }
}
