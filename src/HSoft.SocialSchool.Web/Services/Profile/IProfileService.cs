using HSoft.SocialSchool.Web.Services.Profile.Requests;

namespace HSoft.SocialSchool.Web.Services.Profile;

/// <summary>
/// Perfil propio del usuario autenticado: consulta de <c>GET /api/user/profile/me</c>
/// (200/204/401) y alta con <c>POST /api/user/profile/me</c>.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Consulta el perfil de la sesión actual.
    /// </summary>
    /// <remarks>
    /// Devuelve <see cref="ProfileQueryResult.Success"/> cuando hay perfil (200),
    /// <see cref="ProfileQueryResult.NoContent"/> cuando la cuenta todavía no lo ha
    /// completado (204) y <see cref="ProfileQueryResult.Unauthorized"/> cuando no hay
    /// token (401).
    /// </remarks>
    Task<ProfileQueryResult> GetMyProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>Da de alta el perfil de un deportista.</summary>
    Task<ProfileSaveResult> CreateAthleteProfileAsync(
        CreateAthleteProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Da de alta el perfil de una escuela.</summary>
    Task<ProfileSaveResult> CreateSchoolProfileAsync(
        CreateSchoolProfileRequest request,
        CancellationToken cancellationToken = default);
}
