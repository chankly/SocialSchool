using HSoft.SocialSchool.Api.Models;
using HSoft.SocialSchool.Application.Commands.Registration;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSoft.SocialSchool.Api.Controllers
{
    /// <summary>
    /// Eventos que Keycloak envía a la API. El endpoint es anónimo por decisión del propietario: Keycloak
    /// no firma la llamada y la única entrada que se usa es el identificador del usuario.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class WebHooksController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Recibe un evento de Keycloak. Si es un registro, materializa la cuenta de negocio de ese usuario;
        /// cualquier otro evento se acusa y se ignora.
        /// </summary>
        /// <param name="keycloakEvent">Cuerpo del evento.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        /// <returns><c>200</c> cuando el evento se ha atendido o ignorado; <c>500</c> si la cuenta no se ha
        /// podido crear, para que Keycloak lo reintente.</returns>
        [HttpPost("keycloak")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReceiveWebHook(
            [FromBody] KeycloakEvent? keycloakEvent,
            CancellationToken cancellationToken)
        {
            if (keycloakEvent?.Type != KeycloakEvent.RegistrationType || keycloakEvent.AuthDetails is null)
                return Ok();

            var result = await _mediator.Send(
                new RegisterUserCommand(keycloakEvent.AuthDetails.UserId ?? string.Empty),
                cancellationToken);

            if (result.IsFailure)
                return Problem(
                    detail: result.Error,
                    statusCode: StatusCodes.Status500InternalServerError);

            return Ok();
        }
    }
}
