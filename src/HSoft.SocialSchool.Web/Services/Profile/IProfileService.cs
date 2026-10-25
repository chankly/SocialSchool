using HSoft.SocialSchool.Web.Services.Profile.Requests;

namespace HSoft.SocialSchool.Web.Services.Profile;

/// <summary>
/// Perfil propio del usuario autenticado. El mock reproduce el contrato de
/// <c>GET /api/user/profile/me</c> (200/204/401) y simula también el alta, que en la API
/// real aún no está expuesta.
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

    /// <summary>Simula el alta del perfil de un deportista.</summary>
    Task<ProfileSaveResult> CreateAthleteProfileAsync(
        CreateAthleteProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Simula el alta del perfil de una escuela.</summary>
    Task<ProfileSaveResult> CreateSchoolProfileAsync(
        CreateSchoolProfileRequest request,
        CancellationToken cancellationToken = default);
}
