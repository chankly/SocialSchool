using HSoft.SocialSchool.Web.Services.Profile.Responses;

namespace HSoft.SocialSchool.Web.Services.Profile;

/// <summary>Resultado de crear el perfil en la API simulada.</summary>
public abstract record ProfileSaveResult
{
    private ProfileSaveResult()
    {
    }

    /// <summary>El perfil se ha creado correctamente.</summary>
    public sealed record Success(ProfileResponse Profile) : ProfileSaveResult;

    /// <summary>El nick ya está ocupado; el campo se marca en el formulario.</summary>
    public sealed record Conflict(string Field, string Message) : ProfileSaveResult;

    /// <summary>No hay sesión válida.</summary>
    public sealed record Unauthorized : ProfileSaveResult;

    /// <summary>Fallo simulado de la API.</summary>
    public sealed record Failure(string Message) : ProfileSaveResult;
}
