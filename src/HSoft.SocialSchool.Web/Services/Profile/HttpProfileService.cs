using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HSoft.SocialSchool.Web.Services.Profile.Requests;
using HSoft.SocialSchool.Web.Services.Profile.Responses;

namespace HSoft.SocialSchool.Web.Services.Profile;

/// <summary>
/// Perfil propio contra el API real. Traduce 200/204/401 al resultado que consume la UI
/// y convierte los errores del servidor en mensajes mostrables, sin filtrar detalles técnicos.
/// </summary>
public sealed class HttpProfileService(HttpClient httpClient) : IProfileService
{
    private const string Endpoint = "api/user/profile/me";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <inheritdoc />
    public async Task<ProfileQueryResult> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(Endpoint, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new ProfileQueryResult.Unauthorized();
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new ProfileQueryResult.NoContent();
            }

            if (!response.IsSuccessStatusCode)
            {
                var detail = await ReadProblemDetailAsync(response, cancellationToken);
                return new ProfileQueryResult.Failure(
                    detail ?? "La API no ha podido devolver el perfil.");
            }

            var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(JsonOptions, cancellationToken);
            return profile is null
                ? new ProfileQueryResult.Failure("La API ha devuelto un perfil vacío.")
                : new ProfileQueryResult.Success(profile);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new ProfileQueryResult.Failure(
                "No se ha podido contactar con la API. Comprueba la conexión e inténtalo de nuevo.");
        }
    }

    /// <inheritdoc />
    public Task<ProfileSaveResult> CreateAthleteProfileAsync(
        CreateAthleteProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.DateOfBirth is not { } dateOfBirth)
        {
            return Task.FromResult<ProfileSaveResult>(
                new ProfileSaveResult.Failure("La fecha de nacimiento es obligatoria."));
        }

        var body = new CreateProfileRequestBody(
            request.Nickname.Trim(),
            new CreateAthleteProfileBody(
                request.FirstName.Trim(),
                request.LastName.Trim(),
                DateOnly.FromDateTime(dateOfBirth),
                request.Country.Trim().ToUpperInvariant(),
                request.City.Trim(),
                NormalizeOptional(request.ProfilePictureUrl),
                NormalizeOptional(request.Biography),
                request.PracticedSportIds.ToList()),
            null);

        return CreateProfileAsync(body, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ProfileSaveResult> CreateSchoolProfileAsync(
        CreateSchoolProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var body = new CreateProfileRequestBody(
            request.Nickname.Trim(),
            null,
            new CreateSchoolProfileBody(
                request.Name.Trim(),
                NormalizeOptional(request.LogoUrl),
                request.SportId,
                request.Description.Trim(),
                request.Country.Trim().ToUpperInvariant(),
                request.City.Trim(),
                request.ContactEmail.Trim(),
                NormalizeOptional(request.Phone),
                NormalizeOptional(request.Website)));

        return CreateProfileAsync(body, cancellationToken);
    }

    private async Task<ProfileSaveResult> CreateProfileAsync(
        CreateProfileRequestBody body,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(Endpoint, body, JsonOptions, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new ProfileSaveResult.Unauthorized();
            }

            if (!response.IsSuccessStatusCode)
            {
                var detail = await ReadProblemDetailAsync(response, cancellationToken);
                return MapFailure(response.StatusCode, detail);
            }

            var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(JsonOptions, cancellationToken);
            return profile is null
                ? new ProfileSaveResult.Failure("La API ha guardado el perfil pero no ha devuelto sus datos.")
                : new ProfileSaveResult.Success(profile);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return new ProfileSaveResult.Failure(
                "No se ha podido guardar el perfil. Comprueba la conexión e inténtalo de nuevo.");
        }
    }

    private static ProfileSaveResult MapFailure(HttpStatusCode statusCode, string? detail)
    {
        var message = string.IsNullOrWhiteSpace(detail)
            ? "La API ha rechazado los datos del perfil."
            : detail;

        // El 400 del alta llega con un detail en lenguaje natural. Si habla del nick,
        // se pinta sobre ese campo y el formulario conserva lo que el usuario escribió.
        if (statusCode == HttpStatusCode.BadRequest
            && message.Contains("nick", StringComparison.OrdinalIgnoreCase))
        {
            return new ProfileSaveResult.Conflict(nameof(CreateProfileRequestBody.Nickname), message);
        }

        return new ProfileSaveResult.Failure(message);
    }

    private static async Task<string?> ReadProblemDetailAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetail>(JsonOptions, cancellationToken);
            return string.IsNullOrWhiteSpace(problem?.Detail) ? null : problem.Detail;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record ProblemDetail(string? Detail);
}
