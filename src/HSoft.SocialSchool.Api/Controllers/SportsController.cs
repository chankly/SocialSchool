using HSoft.SocialSchool.Application.Models.Profiles;
using HSoft.SocialSchool.Application.Queries.Sports;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace HSoft.SocialSchool.Api.Controllers
{
    /// <summary>
    /// Catálogo de deportes. Lo mantiene el técnico; la API solo lo publica.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SportsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Devuelve los deportes activos del catálogo para el selector del formulario de perfil.
        /// </summary>
        /// <remarks>
        /// Solo incluye los deportes activos, con su identificador y su nombre. La política de denegación por
        /// defecto exige un usuario autenticado.
        /// </remarks>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        /// <returns>Los deportes activos.</returns>
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<SportResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSports(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSportsQuery(), cancellationToken);

            if (result.IsFailure)
                return Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest);

            return Ok(result.Value);
        }
    }
}
