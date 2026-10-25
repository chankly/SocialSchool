namespace HSoft.SocialSchool.Web.Services.Profile.Responses;

/// <summary>
/// Bloque <c>athlete</c> de la respuesta de perfil. Solo llega relleno cuando la cuenta
/// es de tipo deportista.
/// </summary>
public sealed record AthleteProfileResponse(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Country,
    string City,
    string? ProfilePictureUrl,
    string? Biography,
    List<SportSummary> PracticedSports);
