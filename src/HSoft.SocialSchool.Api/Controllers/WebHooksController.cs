using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Api.Models;
using HSoft.SocialSchool.Application.Commands.Registration;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSoft.SocialSchool.Api.Controllers
{
    /// <summary>
    /// Eventos que Keycloak envía a la API. El endpoint es anónimo por decisión del propietario: Keycloak no
    /// firma la llamada y del cuerpo solo se usan el tipo, el identificador del usuario y, en las
    /// actualizaciones, los campos que cambiaron.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class WebHooksController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Recibe un evento de Keycloak. El alta materializa la cuenta de negocio; la actualización sincroniza
        /// el correo y el idioma. Cualquier otro evento se acusa y se ignora.
        /// </summary>
        /// <param name="keycloakEvent">Cuerpo del evento.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        /// <returns><c>200</c> cuando el evento se ha atendido o ignorado; <c>500</c> si el tratamiento ha
        /// fallado, para que Keycloak lo reintente.</returns>
        [HttpPost("keycloak")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReceiveWebHook(
            [FromBody] KeycloakEvent? keycloakEvent,
            CancellationToken cancellationToken)
        {
            var userId = keycloakEvent?.AuthDetails?.UserId;

            if (string.IsNullOrWhiteSpace(userId))
                return Ok();

            return keycloakEvent!.Type switch
            {
                KeycloakEvent.RegistrationType => ToActionResult(
                    await _mediator.Send(new RegisterUserCommand(userId), cancellationToken)),

                KeycloakEvent.ProfileUpdateType => ToActionResult(
                    await _mediator.Send(
                        new SyncUserProfileCommand(
                            userId,
                            keycloakEvent.Details?.UpdatedEmail,
                            keycloakEvent.Details?.UpdatedLocale),
                        cancellationToken)),

                _ => Ok()
            };
        }

        private IActionResult ToActionResult(Result result) =>
            result.IsFailure
                ? Problem(detail: result.Error, statusCode: StatusCodes.Status500InternalServerError)
                : Ok();
    }
}
