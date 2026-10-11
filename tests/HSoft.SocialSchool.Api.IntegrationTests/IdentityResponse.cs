namespace HSoft.SocialSchool.Api.IntegrationTests
{
    /// <summary>Identity reported by the test endpoint, resolved through <c>ICurrentUser</c>.</summary>
    internal sealed record IdentityResponse(string? ExternalId, bool IsAuthenticated);
}
