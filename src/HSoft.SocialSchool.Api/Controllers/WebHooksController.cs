using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HSoft.SocialSchool.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebHooksController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("keycloak")]
        public async Task<IActionResult> ReceiveWebHook()
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            Console.WriteLine($"Received webhook payload: {payload}");

            // Return a 200 OK response to acknowledge receipt of the webhook
            return Ok();
        }
    }
}
