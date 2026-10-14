using HSoft.SocialSchool.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSoft.SocialSchool.Api.IntegrationTests.Controllers
{
    /// <summary>
    /// Controller that exists only in the test host. It exposes the resolved identity and an endpoint
    /// guarded by a test-only policy, so the authorization pipeline can be exercised without
    /// inventing business endpoints or permissions.
    /// </summary>
    [ApiController]
    [Route("__test")]
    public sealed class TestPolicyController(ICurrentUser currentUser) : ControllerBase
    {
        [HttpGet("identity")]
        public IActionResult GetIdentity() =>
            Ok(new IdentityResponse(currentUser.ExternalId, currentUser.IsAuthenticated));

        [HttpGet("policy")]
        [Authorize(Policy = TestPolicy.PolicyName)]
        public IActionResult GetWithPolicy() => Ok();
    }
}
