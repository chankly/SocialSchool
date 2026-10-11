using HSoft.SocialSchool.Application.Services;
using Microsoft.AspNetCore.Http;

namespace HSoft.SocialSchool.Api.Infrastructure.Security
{
    /// <summary>
    /// Obtiene la identidad del usuario autenticado a partir del contexto HTTP.
    /// </summary>
    public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
    {
        private const string SubjectClaimType = "sub";

        /// <inheritdoc />
        public string? ExternalId => FindClaimValue(SubjectClaimType);

        /// <inheritdoc />
        public bool IsAuthenticated =>
            httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

        // Claim names are kept as Keycloak emits them, so "sub" is read literally.
        private string? FindClaimValue(string claimType) =>
            httpContextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
    }
}
