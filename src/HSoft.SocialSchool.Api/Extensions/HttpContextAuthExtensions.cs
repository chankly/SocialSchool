namespace HSoft.SocialSchool.Api.Extensions
{
    public static class HttpContextAuthExtensions
    {
        /// <summary>
        /// Get the authorization header parts (scheme and credential) from the HTTP context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static (string Scheme, string Credential) GetAuthorizationParts(this HttpContext context)
        {
            var header = context.Request.Headers.Authorization.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(header))
                return (string.Empty, string.Empty);

            var parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return parts.Length == 2
                ? (parts[0], parts[1])
                : (string.Empty, string.Empty);
        }

        /// <summary>
        /// Is the authorization header a reference bearer token (i.e., a token that does not contain a dot, which is typically used for JWT tokens)?
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsReferenceBearerToken(this HttpContext context)
        {
            var (scheme, credential) = context.GetAuthorizationParts();

            return scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase)
                   && !string.IsNullOrEmpty(credential)
                   && !credential.Contains('.');
        }
    }
}
