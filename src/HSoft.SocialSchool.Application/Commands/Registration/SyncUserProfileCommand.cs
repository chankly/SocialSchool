using CSharpFunctionalExtensions;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Registration
{
    /// <summary>
    /// Sincroniza con la cuenta los datos que el usuario ha cambiado en Keycloak. El tipo de cuenta no viaja
    /// aquí: no se puede cambiar por esta vía.
    /// </summary>
    /// <param name="KeycloakUserId">Identificador del usuario en Keycloak.</param>
    /// <param name="Email">Correo nuevo, si lo que cambió fue el correo.</param>
    /// <param name="PreferredLanguage">Idioma nuevo, si lo que cambió fue el idioma.</param>
    public sealed record SyncUserProfileCommand(
        string KeycloakUserId,
        string? Email,
        string? PreferredLanguage) : IRequest<Result>;
}
