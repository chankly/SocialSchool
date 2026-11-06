using System.Net.Http.Json;
using System.Text.Json;
using HSoft.SocialSchool.Web.Services.Sports.Responses;

namespace HSoft.SocialSchool.Web.Services.Sports;

/// <summary>
/// Catálogo de deportes contra <c>GET /api/sports</c>. La API solo devuelve identificador
/// y nombre; la categoría del DTO de presentación queda vacía.
/// </summary>
public sealed class HttpSportsService(HttpClient httpClient) : ISportsService
{
    private const string Endpoint = "api/sports";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<IReadOnlyList<SportResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sports = await httpClient.GetFromJsonAsync<List<SportSummaryBody>>(
            Endpoint,
            JsonOptions,
            cancellationToken);

        if (sports is null)
        {
            return Array.Empty<SportResponse>();
        }

        return sports.Select(sport => new SportResponse(sport.Id, sport.Name, string.Empty)).ToList();
    }

    /// <inheritdoc />
    public async Task<SportResponse?> GetByIdAsync(Guid sportId, CancellationToken cancellationToken = default)
    {
        var sports = await GetAllAsync(cancellationToken);
        return sports.FirstOrDefault(sport => sport.Id == sportId);
    }

    private sealed record SportSummaryBody(Guid Id, string Name);
}
