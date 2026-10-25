namespace HSoft.SocialSchool.Web.Services.Profile.Responses;

/// <summary>
/// Bloque <c>school</c> de la respuesta de perfil. Solo llega relleno cuando la cuenta
/// es de tipo escuela.
/// </summary>
public sealed record SchoolProfileResponse(
    string Name,
    string? LogoUrl,
    Guid SportId,
    string Description,
    string Country,
    string City,
    string ContactEmail,
    string? Phone,
    string? Website);
