using System.Net;
using Xunit;

namespace HSoft.SocialSchool.Api.IntegrationTests
{
    /// <summary>
    /// El navegador manda un <c>OPTIONS</c> antes de la petición real cuando la web vive en otro origen: si
    /// ese preflight no se responde, la web nunca llega a llamar al API aunque el token sea válido.
    /// </summary>
    public class CorsTests
    {
        [Fact]
        public async Task El_preflight_se_permite_desde_cualquier_origen()
        {
            await using var factory = new SocialSchoolApiFactory();
            using var client = factory.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Options, "/api/user/profile/me");
            request.Headers.Add("Origin", "https://web.example");
            request.Headers.Add("Access-Control-Request-Method", "GET");
            request.Headers.Add("Access-Control-Request-Headers", "authorization");

            using var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("*", Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
            Assert.Contains(
                "authorization",
                Assert.Single(response.Headers.GetValues("Access-Control-Allow-Headers")),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
