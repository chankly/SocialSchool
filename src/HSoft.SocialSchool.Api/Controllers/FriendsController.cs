using HSoft.SocialSchool.Application.Commands.Friends;
using HSoft.SocialSchool.Application.Models.Friends;
using HSoft.SocialSchool.Application.Queries.Friends;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace HSoft.SocialSchool.Api.Controllers
{
    /// <summary>
    /// Amistades del usuario autenticado: el resumen de sus listas, enviar solicitudes y resolverlas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FriendsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Devuelve amigos, solicitudes recibidas y enviadas, y las cuentas a las que se puede invitar.
        /// </summary>
        /// <remarks>
        /// Las cuentas invitables excluyen al propio usuario, a sus amigos y a quien ya tiene una solicitud
        /// pendiente con él; se devuelven como máximo veinte.
        /// </remarks>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        /// <returns>El resumen de amistades del usuario autenticado.</returns>
        [HttpGet]
        [ProducesResponseType<FriendsResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFriends(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetFriendsQuery(), cancellationToken);

            if (result.IsFailure)
                return Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest);

            return Ok(result.Value);
        }

        /// <summary>
        /// Envía una solicitud de amistad a otra cuenta.
        /// </summary>
        /// <remarks>
        /// Se rechaza con <c>400</c> si el destinatario es uno mismo, si las dos cuentas son escuelas, si ya
        /// son amigos o si ya hay una solicitud pendiente en cualquier dirección. La solicitud nace pendiente
        /// y no crea la amistad hasta que se acepta.
        /// </remarks>
        /// <param name="command">Cuenta a la que se envía la solicitud.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        /// <returns>El identificador de la solicitud creada.</returns>
        [HttpPost("requests")]
        [ProducesResponseType<FriendRequestCreatedResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendFriendRequest(
            [FromBody] SendFriendRequestCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest);

            return Ok(new FriendRequestCreatedResponse(result.Value));
        }

        /// <summary>
        /// Acepta una solicitud de amistad recibida y crea la amistad.
        /// </summary>
        /// <remarks>
        /// Solo puede hacerlo el destinatario y solo mientras la solicitud esté pendiente; cualquier otra
        /// cuenta recibe <c>400</c> y la solicitud no cambia.
        /// </remarks>
        /// <param name="requestId">Identificador de la solicitud.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        [HttpPost("requests/{requestId:guid}/accept")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AcceptFriendRequest(Guid requestId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new AcceptFriendRequestCommand(requestId), cancellationToken);

            if (result.IsFailure)
                return Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest);

            return NoContent();
        }

        /// <summary>
        /// Rechaza una solicitud de amistad recibida.
        /// </summary>
        /// <remarks>
        /// Solo puede hacerlo el destinatario y solo mientras la solicitud esté pendiente. El rechazo es
        /// silencioso: no se genera ningún aviso y el emisor podrá volver a intentarlo.
        /// </remarks>
        /// <param name="requestId">Identificador de la solicitud.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        [HttpPost("requests/{requestId:guid}/reject")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RejectFriendRequest(Guid requestId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RejectFriendRequestCommand(requestId), cancellationToken);

            if (result.IsFailure)
                return Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest);

            return NoContent();
        }

        /// <summary>
        /// Cancela una solicitud de amistad enviada.
        /// </summary>
        /// <remarks>
        /// Solo puede hacerlo su emisor y solo mientras siga pendiente; cualquier otra cuenta recibe
        /// <c>400</c> y la solicitud no cambia.
        /// </remarks>
        /// <param name="requestId">Identificador de la solicitud.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        [HttpDelete("requests/{requestId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelFriendRequest(Guid requestId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelFriendRequestCommand(requestId), cancellationToken);

            if (result.IsFailure)
                return Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest);

            return NoContent();
        }

        /// <summary>
        /// Deshace la amistad con otra cuenta.
        /// </summary>
        /// <remarks>
        /// Cualquiera de las dos partes puede hacerlo; la amistad desaparece de las listas de ambas.
        /// </remarks>
        /// <param name="userId">La otra cuenta de la amistad.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        [HttpDelete("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveFriend(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RemoveFriendCommand(userId), cancellationToken);

            if (result.IsFailure)
                return Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest);

            return NoContent();
        }
    }
}
