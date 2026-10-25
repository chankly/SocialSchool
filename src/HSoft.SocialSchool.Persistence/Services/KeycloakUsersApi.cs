using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Registration;
using HSoft.SocialSchool.Application.Services;
using Microsoft.Extensions.Configuration;

namespace HSoft.SocialSchool.Persistence.Services
{
    /// <summary>
    /// Consulta usuarios en la Admin API de Keycloak. Pide un token con <c>client_credentials</c> en cada
    /// llamada y deriva las dos URLs de <c>Keycloak:Authority</c>, que es lo que la API ya usa para validar
    /// tokens: así el realm no se duplica en la configuración.
    /// </summary>
    public sealed class KeycloakUsersApi : IKeycloakUsersApi
    {
        private const string TokenPath = "/protocol/openid-connect/token";
        private const string RealmsSegment = "/realms/";
        private const string AdminRealmsSegment = "/admin/realms/";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public KeycloakUsersApi(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<Result<KeycloakUserProfile?>> GetUserAsync(
            string keycloakUserId,
            CancellationToken cancellationToken = default)
        {
            var authority = _configuration["Keycloak:Authority"];
            var clientId = _configuration["Keycloak:Admin:ClientId"];
            var clientSecret = _configuration["Keycloak:Admin:ClientSecret"];

            if (string.IsNullOrWhiteSpace(authority))
                return Result.Failure<KeycloakUserProfile?>("Keycloak:Authority is not configured.");

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                return Result.Failure<KeycloakUserProfile?>("The Keycloak admin client credentials are not configured.");

            string accessToken;

            try
            {
                accessToken = await GetAccessTokenAsync(authority, clientId, clientSecret, cancellationToken);
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                return Result.Failure<KeycloakUserProfile?>($"Could not get a Keycloak token: {exception.Message}");
            }

            var userUrl = $"{BuildAdminAuthority(authority)}/users/{Uri.EscapeDataString(keycloakUserId)}";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, userUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return Result.Success<KeycloakUserProfile?>(null);

                if (!response.IsSuccessStatusCode)
                    return Result.Failure<KeycloakUserProfile?>(
                        $"Keycloak answered {(int)response.StatusCode} when reading the user.");

                var representation = await response.Content
                    .ReadFromJsonAsync<KeycloakUserRepresentation>(cancellationToken);

                return Result.Success(representation is null ? null : Map(representation));
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                return Result.Failure<KeycloakUserProfile?>($"Could not read the Keycloak user: {exception.Message}");
            }
        }

        private async Task<string> GetAccessTokenAsync(
            string authority,
            string clientId,
            string clientSecret,
            CancellationToken cancellationToken)
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            });

            using var response = await _httpClient.PostAsync(
                $"{authority.TrimEnd('/')}{TokenPath}",
                content,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
                ?? throw new HttpRequestException("Keycloak returned an empty token response.");

            return token.AccessToken
                ?? throw new HttpRequestException("Keycloak returned no access token.");
        }

        /// <summary>
        /// La Admin API cuelga de <c>/admin/realms/{realm}</c>; la autoridad es <c>.../realms/{realm}</c>.
        /// </summary>
        private static string BuildAdminAuthority(string authority)
        {
            var trimmed = authority.TrimEnd('/');

            return trimmed.Contains(RealmsSegment, StringComparison.OrdinalIgnoreCase)
                ? trimmed.Replace(RealmsSegment, AdminRealmsSegment, StringComparison.OrdinalIgnoreCase)
                : $"{trimmed}{AdminRealmsSegment.TrimStart('/')}";
        }

        private static KeycloakUserProfile Map(KeycloakUserRepresentation user) => new(
            user.Id ?? string.Empty,
            user.Email ?? string.Empty,
            FirstAttribute(user.Attributes, "userType"),
            user.FederatedIdentities?.FirstOrDefault()?.IdentityProvider,
            FirstAttribute(user.Attributes, "locale"));

        private static string? FirstAttribute(
            Dictionary<string, string[]>? attributes,
            string name) =>
            attributes is not null && attributes.TryGetValue(name, out var values)
                ? values.FirstOrDefault()
                : null;

        private sealed class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }
        }

        /// <summary>Lo que se necesita del usuario de Keycloak; el resto de la representación se ignora.</summary>
        private sealed class KeycloakUserRepresentation
        {
            public string? Id { get; set; }

            public string? Email { get; set; }

            public Dictionary<string, string[]>? Attributes { get; set; }

            public List<FederatedIdentity>? FederatedIdentities { get; set; }

            public sealed class FederatedIdentity
            {
                public string? IdentityProvider { get; set; }
            }
        }
    }
}
