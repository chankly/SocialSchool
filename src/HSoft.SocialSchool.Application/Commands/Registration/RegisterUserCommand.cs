using CSharpFunctionalExtensions;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Registration
{
    /// <summary>
    /// Materializa la cuenta de negocio del usuario que acaba de registrarse en Keycloak.
    /// </summary>
    /// <param name="KeycloakUserId">Identificador del usuario en Keycloak (el <c>sub</c> del token).</param>
    public sealed record RegisterUserCommand(string KeycloakUserId) : IRequest<Result>;
}
