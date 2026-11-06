namespace HSoft.SocialSchool.Web.Services.Profile.Requests;

/// <summary>
/// Cuerpo de <c>POST /api/user/profile/me</c>. El servidor exige exactamente uno de los
/// dos bloques, el que corresponde al tipo de la cuenta autenticada.
/// </summary>
/// <param name="Nickname">Nick único e inmutable que se fija en el alta.</param>
/// <param name="Athlete">Datos del deportista, o <see langword="null"/> si la cuenta es una escuela.</param>
/// <param name="School">Datos de la escuela, o <see langword="null"/> si la cuenta es un deportista.</param>
public sealed record CreateProfileRequestBody(
    string Nickname,
    CreateAthleteProfileBody? Athlete,
    CreateSchoolProfileBody? School);

/// <summary>Bloque <c>athlete</c> del alta de perfil.</summary>
public sealed record CreateAthleteProfileBody(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Country,
    string City,
    string? ProfilePictureUrl,
    string? Biography,
    IReadOnlyList<Guid> PracticedSportIds);

/// <summary>Bloque <c>school</c> del alta de perfil.</summary>
public sealed record CreateSchoolProfileBody(
    string Name,
    string? LogoUrl,
    Guid SportId,
    string Description,
    string Country,
    string City,
    string ContactEmail,
    string? Phone,
    string? Website);
