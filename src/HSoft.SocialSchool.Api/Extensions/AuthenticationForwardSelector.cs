namespace HSoft.SocialSchool.Api.Extensions
{
    public static class AuthenticationForwardSelector
    {
        /// <summary>
        /// Forward the authentication to the introspection scheme if the token is a reference bearer token (i.e., a token that does not contain a dot, which is typically used for JWT tokens). Otherwise, do not forward the authentication.
        /// </summary>
        /// <param name="introspectionScheme"></param>
        /// <returns></returns>
        public static Func<HttpContext, string?> ForwardReferenceToken(string introspectionScheme = "Introspection")
        {
            return context =>
                context.IsReferenceBearerToken()
                    ? introspectionScheme
                    : null;
        }
    }
}
