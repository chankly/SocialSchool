namespace HSoft.SocialSchool.Api.IntegrationTests
{
    /// <summary>
    /// Authorization policy that exists only in the test host. It lets the tests exercise a 403
    /// without inventing business roles or permissions in production.
    /// </summary>
    internal static class TestPolicy
    {
        internal const string PolicyName = "test.require-claim";
        internal const string ClaimType = "test_role";
        internal const string ClaimValue = "allowed";
    }
}
