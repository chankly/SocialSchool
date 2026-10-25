namespace HSoft.SocialSchool.Api.Models
{
    /// <summary>
    /// Cambios que anuncia un evento de actualización de perfil. Solo se modelan los dos que afectan a la
    /// cuenta; el objeto cambia según lo que el usuario haya editado (idioma, correo, nombre, apellidos…).
    /// </summary>
    /// <param name="UpdatedEmail">Correo nuevo, si lo que cambió fue el correo.</param>
    /// <param name="UpdatedLocale">Idioma nuevo, si lo que cambió fue el idioma.</param>
    public sealed record KeycloakEventChange(string? UpdatedEmail, string? UpdatedLocale);
}
