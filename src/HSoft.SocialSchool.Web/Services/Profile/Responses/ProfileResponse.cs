using HSoft.SocialSchool.Web.Models.Accounts;

namespace HSoft.SocialSchool.Web.Services.Profile.Responses;

/// <summary>
/// Respuesta 200 de <c>GET /api/user/profile/me</c>. Solo uno de los bloques
/// <see cref="Athlete"/> o <see cref="School"/> viene relleno.
/// </summary>
public sealed record ProfileResponse(
    Guid Id,
    string Nickname,
    string Email,
    AccountType AccountType,
    AccountStatus AccountStatus,
    AthleteProfileResponse? Athlete,
    SchoolProfileResponse? School);
