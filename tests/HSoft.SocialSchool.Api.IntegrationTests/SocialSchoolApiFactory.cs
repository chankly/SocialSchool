using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HSoft.SocialSchool.Api.IntegrationTests
{
    /// <summary>
    /// Test host that replaces the Keycloak token validation with a local signing key, so the API
    /// pipeline can be exercised without a running Keycloak. The real configuration is pinned by
    /// <see cref="AuthenticationConfigurationTests"/>.
    /// </summary>
    public sealed class SocialSchoolApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = null;
                    options.RequireHttpsMetadata = false;
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = TestTokens.Issuer,
                        ValidateAudience = true,
                        ValidAudience = TestTokens.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = TestTokens.SecurityKey,
                        ClockSkew = TimeSpan.Zero
                    };
                });

                services.AddAuthorization(options => options.AddPolicy(
                    TestPolicy.PolicyName,
                    policy => policy.RequireClaim(TestPolicy.ClaimType, TestPolicy.ClaimValue)));

                // Registers the controller used only by the tests.
                services.AddControllers().AddApplicationPart(typeof(SocialSchoolApiFactory).Assembly);
            });
        }
    }
}
