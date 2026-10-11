using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests
{
    /// <summary>
    /// Exercises authentication and authorization end to end through the middleware pipeline.
    /// Tokens are signed with the test key, so this does not prove the real Keycloak integration.
    /// </summary>
    public class AuthorizationPipelineTests : IClassFixture<SocialSchoolApiFactory>
    {
        private readonly SocialSchoolApiFactory _factory;

        public AuthorizationPipelineTests(SocialSchoolApiFactory factory) => _factory = factory;

        [Fact]
        public async Task Request_without_token_is_rejected()
        {
            var response = await CreateClient().GetAsync("/__test/identity");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Existing_endpoint_without_token_is_rejected()
        {
            var response = await CreateClient().PutAsync("/api/management/migration/apply", content: null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Valid_token_reaches_the_endpoint_and_resolves_the_subject()
        {
            // The token carries no optional claims (no email, name or preferred_username): identity
            // must come from "sub" alone.
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = Bearer(TestTokens.Create("user-123"));

            var response = await client.GetAsync("/__test/identity");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var identity = await response.Content.ReadFromJsonAsync<IdentityResponse>();
            Assert.NotNull(identity);
            Assert.Equal("user-123", identity!.ExternalId);
            Assert.True(identity.IsAuthenticated);
        }

        [Fact]
        public async Task Expired_token_is_rejected()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = Bearer(TestTokens.Create(
                "user-123",
                notBefore: TimeSpan.FromMinutes(-20),
                expiresIn: TimeSpan.FromMinutes(10)));

            var response = await client.GetAsync("/__test/identity");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Token_from_another_issuer_is_rejected()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = Bearer(TestTokens.Create(
                "user-123",
                issuer: TestTokens.OtherIssuer));

            var response = await client.GetAsync("/__test/identity");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Token_issued_for_another_client_is_rejected()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = Bearer(TestTokens.Create(
                "user-123",
                audience: TestTokens.OtherAudience));

            var response = await client.GetAsync("/__test/identity");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Token_without_the_required_claim_is_forbidden()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = Bearer(TestTokens.Create("user-123"));

            var response = await client.GetAsync("/__test/policy");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Token_with_the_required_claim_is_allowed()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = Bearer(TestTokens.Create(
                "user-123",
                extraClaims: new Dictionary<string, string>
                {
                    [TestPolicy.ClaimType] = TestPolicy.ClaimValue
                }));

            var response = await client.GetAsync("/__test/policy");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private HttpClient CreateClient() =>
            _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        private static AuthenticationHeaderValue Bearer(string token) => new("Bearer", token);
    }
}
