using HSoft.SocialSchool.Application.Commands.Migrations;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace HSoft.SocialSchool.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagementController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPut("migration/apply")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Put()
        {
            var response = await _mediator.Send(new ApplyMigrationCommand());

            if (response.IsFailure) return Problem(detail: response.Error);

            return Ok();
        }
    }
}
