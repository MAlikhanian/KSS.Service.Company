using System.Net;
using System.Net.Http.Json;
using KSS.Dto;
using KSS.Service.Client;
using KSS.Service.Service;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KSS.Api.Tests
{
    /// <summary>Company display-name selection: a pure function, tested without a database.</summary>
    public class CompanyDisplayNameTests
    {
        private const string RegistryId = "10100000001";

        private static string Pick(short requested, params (short LanguageId, string? Name)[] translations) =>
            CompanyDisplayName.PickCompanyDisplayName(translations, requested, RegistryId);

        [Fact]
        public void The_requested_language_wins() =>
            Assert.Equal("English name", Pick(10, (12, "Persian name"), (10, "English name")));

        [Fact]
        public void Falls_back_to_Persian_when_the_requested_language_is_missing() =>
            Assert.Equal("Persian name", Pick(99, (10, "English name"), (12, "Persian name")));

        [Fact]
        public void Falls_back_to_English_when_there_is_no_Persian_name() =>
            Assert.Equal("English name", Pick(99, (30, "Other name"), (10, "English name")));

        [Fact]
        public void Falls_back_to_the_lowest_remaining_language() =>
            Assert.Equal("Name 30", Pick(99, (40, "Name 40"), (30, "Name 30")));

        [Fact]
        public void Blank_names_are_skipped() =>
            Assert.Equal("Name 30", Pick(10, (10, "   "), (12, ""), (30, "Name 30")));

        [Fact]
        public void The_registry_id_is_used_only_when_no_language_has_a_name()
        {
            Assert.Equal(RegistryId, Pick(10, (10, " "), (12, null)));
            Assert.Equal(RegistryId, Pick(10));
        }

        [Fact]
        public void The_choice_does_not_depend_on_input_order()
        {
            var a = Pick(99, (40, "Name 40"), (30, "Name 30"), (50, "Name 50"));
            var b = Pick(99, (50, "Name 50"), (30, "Name 30"), (40, "Name 40"));
            Assert.Equal(a, b);
        }
    }

    /// <summary>
    /// Person name resolution through the real client code, with a stub HTTP handler (no
    /// network). A person without a name is left out; the national id is never a name.
    /// </summary>
    public class PersonDisplayNameTests
    {
        private const string NationalId = "0012345678";

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            private readonly List<PersonNameDto> _body;
            public int Calls { get; private set; }

            public StubHandler(HttpStatusCode status, List<PersonNameDto> body)
            {
                _status = status;
                _body = body;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Calls++;
                return Task.FromResult(new HttpResponseMessage(_status) { Content = JsonContent.Create(_body) });
            }
        }

        private static async Task<IReadOnlyDictionary<Guid, string>> Resolve(HttpStatusCode status, params PersonNameDto[] people)
        {
            var handler = new StubHandler(status, people.ToList());
            var http = new HttpClient(handler) { BaseAddress = new Uri("http://person.service.test/") };
            var client = new PersonApiClient(http, NullLogger<PersonApiClient>.Instance);
            var result = await client.GetPersonNamesAsync(people.Select(p => p.Id).Append(Guid.NewGuid()).ToList());
            Assert.Equal(1, handler.Calls);
            return result;
        }

        [Fact]
        public async Task A_person_with_no_name_is_left_out_and_the_national_id_appears_nowhere()
        {
            var unnamed = new PersonNameDto { Id = Guid.NewGuid(), FirstName = " ", LastName = "", NationalId = NationalId };

            var result = await Resolve(HttpStatusCode.OK, unnamed);

            Assert.False(result.ContainsKey(unnamed.Id));
            Assert.DoesNotContain(result.Values, v => v.Contains(NationalId, StringComparison.Ordinal));
        }

        [Fact]
        public async Task A_named_person_resolves_to_first_and_last_name()
        {
            var named = new PersonNameDto { Id = Guid.NewGuid(), FirstName = "First", LastName = "Last", NationalId = NationalId };

            var result = await Resolve(HttpStatusCode.OK, named);

            Assert.Equal("First Last", result[named.Id]);
            Assert.DoesNotContain(result.Values, v => v.Contains(NationalId, StringComparison.Ordinal));
        }

        [Fact]
        public async Task Named_and_unnamed_together()
        {
            var named = new PersonNameDto { Id = Guid.NewGuid(), FirstName = "First", LastName = "Last", NationalId = "0000000001" };
            var unnamed = new PersonNameDto { Id = Guid.NewGuid(), NationalId = NationalId };

            var result = await Resolve(HttpStatusCode.OK, named, unnamed);

            Assert.Equal(new[] { named.Id }, result.Keys.ToArray());
            Assert.DoesNotContain(result.Values, v => v.Contains(NationalId, StringComparison.Ordinal));
        }

        [Fact]
        public async Task A_failed_call_leaves_every_name_unresolved()
        {
            var named = new PersonNameDto { Id = Guid.NewGuid(), FirstName = "First", LastName = "Last" };

            var result = await Resolve(HttpStatusCode.InternalServerError, named);

            Assert.Empty(result);
        }
    }
}
