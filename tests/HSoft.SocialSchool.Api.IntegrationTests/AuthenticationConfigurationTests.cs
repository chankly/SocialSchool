using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests
{
    /// <summary>
    /// Pins the real token validation configuration. Unlike the pipeline tests, this uses the host as
    /// it is configured from appsettings.json, with no test override.
    /// </summary>
    public class AuthenticationConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private const string ExpectedAudience = "F6EB144A-BFDB-4FBC-B5F8-1E223E782D6B";

        private readonly WebApplicationFactory<Program> _factory;

        public AuthenticationConfigurationTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public void JwtBearer_validates_issuer_and_audience_against_keycloak()
        {
            var options = GetJwtBearerOptions();

            // Authority must come from the Keycloak:Authority key, not from the template's
            // securityServer:authority.
            var configuredAuthority = _factory.Services
                .GetRequiredService<IConfiguration>()["Keycloak:Authority"];

            Assert.False(string.IsNullOrWhiteSpace(configuredAuthority));
            Assert.Equal(configuredAuthority, options.Authority);
            Assert.Equal(ExpectedAudience, options.Audience);
            Assert.Equal(options.Audience, options.TokenValidationParameters.ValidAudience);
        }

        [Fact]
        public void JwtBearer_keeps_the_original_claim_names()
        {
            Assert.False(GetJwtBearerOptions().MapInboundClaims);
        }

        [Fact]
        public void JwtBearer_does_not_require_the_rfc9068_token_type()
        {
            var validTypes = GetJwtBearerOptions().TokenValidationParameters.ValidTypes ?? [];

            Assert.DoesNotContain("at+jwt", validTypes);
        }

        [Fact]
        public async Task Only_the_jwt_bearer_scheme_is_registered()
        {
            var provider = _factory.Services.GetRequiredService<IAuthenticationSchemeProvider>();

            Assert.Null(await provider.GetSchemeAsync("Introspection"));

            var defaultScheme = await provider.GetDefaultAuthenticateSchemeAsync();
            Assert.Equal(JwtBearerDefaults.AuthenticationScheme, defaultScheme?.Name);
        }

        private JwtBearerOptions GetJwtBearerOptions() =>
            _factory.Services
                .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                .Get(JwtBearerDefaults.AuthenticationScheme);
    }
}
