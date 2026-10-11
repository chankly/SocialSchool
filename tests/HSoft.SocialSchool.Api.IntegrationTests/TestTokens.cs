using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HSoft.SocialSchool.Api.IntegrationTests
{
    /// <summary>
    /// Issues access tokens signed with a local key, so the tests can exercise the API pipeline
    /// without a running Keycloak. It does <b>not</b> verify the real Keycloak integration: the claim
    /// validity of a real realm is checked manually.
    /// </summary>
    internal static class TestTokens
    {
        internal const string Issuer = "https://keycloak.tests/realms/socialschool";
        internal const string Audience = "SocialSchool-Api";
        internal const string OtherIssuer = "https://keycloak.tests/realms/other";
        internal const string OtherAudience = "SocialSchool-Web";

        private static readonly SymmetricSecurityKey Key =
            new(Encoding.UTF8.GetBytes("integration-tests-signing-key-0123456789"));

        internal static SecurityKey SecurityKey => Key;

        internal static string Create(
            string subject,
            string? clientId = "SocialSchool-Web",
            IReadOnlyDictionary<string, string>? extraClaims = null,
            string issuer = Issuer,
            string audience = Audience,
            TimeSpan? notBefore = null,
            TimeSpan? expiresIn = null)
        {
            var claims = new List<Claim> { new("sub", subject) };

            if (clientId is not null)
                claims.Add(new Claim("azp", clientId));

            if (extraClaims is not null)
                claims.AddRange(extraClaims.Select(claim => new Claim(claim.Key, claim.Value)));

            var now = DateTime.UtcNow;
            var validFrom = now.Add(notBefore ?? TimeSpan.Zero);
            var expires = validFrom.Add(expiresIn ?? TimeSpan.FromMinutes(5));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: validFrom,
                expires: expires,
                signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
