using HSoft.SocialSchool.Web.Models.Accounts;
using HSoft.SocialSchool.Web.Services.Demo;
using HSoft.SocialSchool.Web.Services.Profile.Requests;
using HSoft.SocialSchool.Web.Services.Profile.Responses;
using HSoft.SocialSchool.Web.Services.Session;
using HSoft.SocialSchool.Web.Services.Sports;

namespace HSoft.SocialSchool.Web.Services.Profile;

/// <summary>
/// Perfil simulado en memoria. Reproduce los estados 200/204/401 del endpoint real y
/// añade el alta de perfil que la maqueta necesita para que el flujo sea completo.
/// </summary>
public sealed class MockProfileService(
    ISessionService session,
    MockDataStore dataStore,
    ISportsService sportsService) : IProfileService
{
    private static readonly TimeSpan Latency = TimeSpan.FromMilliseconds(320);

    /// <inheritdoc />
    public async Task<ProfileQueryResult> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        var isAuthenticated = session.IsAuthenticated;
        var userId = session.UserId;

        await Task.Delay(Latency, cancellationToken);

        if (!isAuthenticated)
        {
            return new ProfileQueryResult.Unauthorized();
        }

        if (dataStore.TryGetProfile(userId, out var profile) && profile is not null)
        {
            return new ProfileQueryResult.Success(profile);
        }

        return new ProfileQueryResult.NoContent();
    }

    /// <inheritdoc />
    public async Task<ProfileSaveResult> CreateAthleteProfileAsync(
        CreateAthleteProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var isAuthenticated = session.IsAuthenticated;
        var accountType = session.AccountType;
        var userId = session.UserId;
        var email = session.Email;
        var accountStatus = session.AccountStatus;

        await Task.Delay(Latency, cancellationToken);

        if (!isAuthenticated)
        {
            return new ProfileSaveResult.Unauthorized();
        }

        if (accountType != AccountType.Athlete)
        {
            return new ProfileSaveResult.Failure("La sesión actual no es de tipo deportista.");
        }

        var nickname = request.Nickname.Trim();
        if (dataStore.IsNicknameTaken(nickname))
        {
            return new ProfileSaveResult.Conflict(nameof(request.Nickname), "Ese nick ya está en uso. Elige otro.");
        }

        var catalog = await sportsService.GetAllAsync(cancellationToken);
        var practicedSports = catalog
            .Where(sport => request.PracticedSportIds.Contains(sport.Id))
            .Select(sport => new SportSummary(sport.Id, sport.Name))
            .ToList();

        if (practicedSports.Count == 0)
        {
            return new ProfileSaveResult.Failure("Selecciona al menos un deporte que practiques.");
        }

        var dateOfBirth = request.DateOfBirth;
        if (dateOfBirth is null)
        {
            return new ProfileSaveResult.Failure("La fecha de nacimiento es obligatoria.");
        }

        var profile = new ProfileResponse(
            userId,
            nickname,
            email,
            AccountType.Athlete,
            accountStatus ?? AccountStatus.Active,
            new AthleteProfileResponse(
                request.FirstName.Trim(),
                request.LastName.Trim(),
                DateOnly.FromDateTime(dateOfBirth.Value),
                request.Country.Trim().ToUpperInvariant(),
                request.City.Trim(),
                NormalizeOptional(request.ProfilePictureUrl),
                NormalizeOptional(request.Biography),
                practicedSports),
            null);

        dataStore.SaveProfile(profile);
        return new ProfileSaveResult.Success(profile);
    }

    /// <inheritdoc />
    public async Task<ProfileSaveResult> CreateSchoolProfileAsync(
        CreateSchoolProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var isAuthenticated = session.IsAuthenticated;
        var accountType = session.AccountType;
        var userId = session.UserId;
        var email = session.Email;
        var accountStatus = session.AccountStatus;

        await Task.Delay(Latency, cancellationToken);

        if (!isAuthenticated)
        {
            return new ProfileSaveResult.Unauthorized();
        }

        if (accountType != AccountType.School)
        {
            return new ProfileSaveResult.Failure("La sesión actual no es de tipo escuela.");
        }

        var nickname = request.Nickname.Trim();
        if (dataStore.IsNicknameTaken(nickname))
        {
            return new ProfileSaveResult.Conflict(nameof(request.Nickname), "Ese nick ya está en uso. Elige otro.");
        }

        var sport = await sportsService.GetByIdAsync(request.SportId, cancellationToken);
        if (sport is null)
        {
            return new ProfileSaveResult.Failure("El deporte seleccionado ya no está disponible.");
        }

        var profile = new ProfileResponse(
            userId,
            nickname,
            email,
            AccountType.School,
            accountStatus ?? AccountStatus.PendingValidation,
            null,
            new SchoolProfileResponse(
                request.Name.Trim(),
                NormalizeOptional(request.LogoUrl),
                request.SportId,
                request.Description.Trim(),
                request.Country.Trim().ToUpperInvariant(),
                request.City.Trim(),
                request.ContactEmail.Trim(),
                NormalizeOptional(request.Phone),
                NormalizeOptional(request.Website)));

        dataStore.SaveProfile(profile);
        return new ProfileSaveResult.Success(profile);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
