using HSoft.SocialSchool.Web.Services.Profile.Responses;

namespace HSoft.SocialSchool.Web.Services.Profile;

/// <summary>
/// Resultado de consultar el perfil propio, con los tres estados que contempla la API:
/// 200, 204 y 401.
/// </summary>
public abstract record ProfileQueryResult
{
    private ProfileQueryResult()
    {
    }

    /// <summary>Hay perfil: la API habría respondido 200.</summary>
    public sealed record Success(ProfileResponse Profile) : ProfileQueryResult;

    /// <summary>No hay perfil todavía: la API habría respondido 204.</summary>
    public sealed record NoContent : ProfileQueryResult;

    /// <summary>No hay token válido: la API habría respondido 401.</summary>
    public sealed record Unauthorized : ProfileQueryResult;

    /// <summary>Fallo de comunicación con la API simulada.</summary>
    public sealed record Failure(string Message) : ProfileQueryResult;
}
