using HSoft.SocialSchool.Application.Models.Profiles;
using HSoft.SocialSchool.Application.Queries.Profiles;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace HSoft.SocialSchool.Api.Controllers
{
    /// <summary>
    /// Perfil del usuario autenticado.
    /// </summary>
    [Route("api/user/profile")]
    [ApiController]
    public class UserProfileController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Devuelve el perfil del usuario autenticado, sea deportista o escuela, según el tipo de su cuenta.
        /// </summary>
        /// <remarks>
        /// La web usa esta respuesta para decidir si muestra el perfil o el formulario de alta: un
        /// <c>204</c> significa que todavía no hay perfil que mostrar, tanto si la cuenta de negocio aún no
        /// existe como si existe sin rellenar.
        /// </remarks>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        /// <returns>El perfil, o <c>204</c> si todavía no existe.</returns>
        [HttpGet("me")]
        [ProducesResponseType<MyProfileResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetMyProfileQuery(), cancellationToken);

            if (result.IsFailure) return Problem(detail: result.Error);

            if (result.Value is null) return NoContent();

            return Ok(result.Value);
        }
    }
}
