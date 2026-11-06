using HSoft.SocialSchool.Application.Models.Schools;
using HSoft.SocialSchool.Application.Queries.Schools;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace HSoft.SocialSchool.Api.Controllers
{
    /// <summary>
    /// Directorio y ficha pública de las escuelas. Cualquier usuario autenticado puede consultarlos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SchoolsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Devuelve el directorio de escuelas, ordenado por nombre.
        /// </summary>
        /// <remarks>
        /// Los tres filtros son opcionales y se pueden combinar. La búsqueda por texto no distingue
        /// mayúsculas. Las escuelas pendientes de validación aparecen con su estado para que el cliente pueda
        /// indicarlo. La política de denegación por defecto exige un usuario autenticado.
        /// </remarks>
        /// <param name="sportId">Deporte por el que filtrar, si se indica.</param>
        /// <param name="city">Ciudad por la que filtrar, si se indica.</param>
        /// <param name="search">Texto que debe contener el nombre, si se indica.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        /// <returns>Las escuelas del directorio, como máximo cincuenta.</returns>
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<SchoolSummaryResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSchools(
            [FromQuery] Guid? sportId,
            [FromQuery] string? city,
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSchoolsQuery(sportId, city, search), cancellationToken);

            if (result.IsFailure)
                return Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest);

            return Ok(result.Value);
        }

        /// <summary>
        /// Devuelve la ficha de una escuela con sus servicios y sus asistentes.
        /// </summary>
        /// <remarks>
        /// Los asistentes solo incluyen las asistencias aceptadas y se publican con nick y foto. La política de
        /// denegación por defecto exige un usuario autenticado.
        /// </remarks>
        /// <param name="id">Identificador de la escuela.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        /// <returns>La ficha de la escuela; <c>404</c> si el identificador no es de una escuela.</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType<SchoolDetailResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSchool(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSchoolQuery(id), cancellationToken);

            if (result.IsFailure)
                return Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest);

            if (result.Value is null)
                return NotFound();

            return Ok(result.Value);
        }
    }
}
