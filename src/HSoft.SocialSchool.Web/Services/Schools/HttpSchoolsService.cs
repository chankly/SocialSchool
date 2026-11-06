using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HSoft.SocialSchool.Web.Models.Accounts;
using HSoft.SocialSchool.Web.Services.Common;
using HSoft.SocialSchool.Web.Services.Schools.Responses;

namespace HSoft.SocialSchool.Web.Services.Schools;

/// <summary>
/// Directorio y ficha pública de escuelas contra <c>/api/schools</c>. El listado solo trae
/// nombre, logo, deporte y ciudad; el resto de datos se piden en la ficha. Un <c>404</c> se
/// traduce a <see langword="null"/> para que la página muestre el estado vacío.
/// </summary>
public sealed class HttpSchoolsService(HttpClient httpClient) : ISchoolsService
{
    private const string Endpoint = "api/schools";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <inheritdoc />
    public async Task<IReadOnlyList<SchoolListItemResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(Endpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await ProblemDetailsReader.ReadDetailAsync(response, cancellationToken);
            throw new HttpRequestException(detail ?? "No se ha podido cargar el directorio de escuelas.");
        }

        var schools = await response.Content.ReadFromJsonAsync<List<SchoolBody>>(JsonOptions, cancellationToken);

        return (schools ?? []).Select(MapListItem).ToList();
    }

    /// <inheritdoc />
    public async Task<SchoolDetailResponse?> GetByIdAsync(Guid schoolId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"{Endpoint}/{schoolId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var detail = await ProblemDetailsReader.ReadDetailAsync(response, cancellationToken);
            throw new HttpRequestException(detail ?? "No se ha podido cargar la escuela.");
        }

        var body = await response.Content.ReadFromJsonAsync<SchoolDetailBody>(JsonOptions, cancellationToken);

        return body is null ? null : MapDetail(body);
    }

    private static SchoolListItemResponse MapListItem(SchoolBody body) => new(
        body.Id,
        body.Name ?? string.Empty,
        body.LogoUrl,
        body.SportId,
        body.SportName ?? string.Empty,
        body.City ?? string.Empty);

    private static SchoolDetailResponse MapDetail(SchoolDetailBody body)
    {
        var services = (body.Services ?? [])
            .Select(service => new SchoolServiceResponse(
                service.Id,
                service.Name ?? string.Empty,
                service.Description,
                service.Price,
                service.Currency))
            .ToList();

        var attendees = (body.Attendees ?? [])
            .Select(attendee => new SchoolAttendeeResponse(
                attendee.Id,
                attendee.Nickname ?? string.Empty,
                attendee.ProfilePictureUrl))
            .ToList();

        return new SchoolDetailResponse(
            body.Id,
            body.Name ?? string.Empty,
            body.LogoUrl,
            body.SportId,
            body.SportName ?? string.Empty,
            body.Description,
            body.Country ?? string.Empty,
            body.City ?? string.Empty,
            body.ContactEmail,
            body.Phone,
            body.Website,
            body.AccountStatus,
            services,
            attendees);
    }

    private sealed record SchoolBody(
        Guid Id,
        string? Name,
        string? LogoUrl,
        Guid SportId,
        string? SportName,
        string? City);

    private sealed record SchoolDetailBody(
        Guid Id,
        string? Name,
        string? LogoUrl,
        Guid SportId,
        string? SportName,
        string? Description,
        string? Country,
        string? City,
        string? ContactEmail,
        string? Phone,
        string? Website,
        AccountStatus? AccountStatus,
        List<ServiceBody>? Services,
        List<AttendeeBody>? Attendees);

    private sealed record ServiceBody(
        Guid Id,
        string? Name,
        string? Description,
        decimal? Price,
        string? Currency);

    private sealed record AttendeeBody(
        Guid Id,
        string? Nickname,
        string? ProfilePictureUrl);
}
