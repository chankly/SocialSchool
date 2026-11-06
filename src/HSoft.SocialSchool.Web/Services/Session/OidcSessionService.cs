using System.Security.Claims;
using HSoft.SocialSchool.Web.Models.Accounts;
using Microsoft.AspNetCore.Components.Authorization;

namespace HSoft.SocialSchool.Web.Services.Session;

/// <summary>
/// Sesión respaldada por el token de Keycloak. La identidad se materializa una vez que
/// el proveedor de autenticación la resuelve; el tipo de cuenta se lee del claim
/// <c>userType</c> y se normaliza con <see cref="AccountTypeParser"/>.
/// </summary>
public sealed class OidcSessionService(AuthenticationStateProvider authenticationStateProvider) : ISessionService
{
    private ClaimsPrincipal? _user;

    /// <inheritdoc />
    public event Action? Changed;

    /// <inheritdoc />
    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public Guid UserId => Guid.TryParse(FindClaim("sub"), out var userId) ? userId : Guid.Empty;

    /// <inheritdoc />
    public string Email => FindClaim("email") ?? string.Empty;

    /// <inheritdoc />
    public string DisplayName =>
        FindClaim("name")
        ?? FindClaim("preferred_username")
        ?? (IsAuthenticated ? Email : "Sin sesión");

    /// <inheritdoc />
    public AccountType? AccountType => AccountTypeParser.TryParse(RawUserType, out var accountType)
        ? accountType
        : null;

    /// <inheritdoc />
    public AccountStatus? AccountStatus => Enum.TryParse<AccountStatus>(
        FindClaim("accountStatus"),
        ignoreCase: true,
        out var status)
        ? status
        : null;

    /// <inheritdoc />
    public string? RawUserType => FindClaim("userType");

    /// <inheritdoc />
    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();

        if (ReferenceEquals(_user, state.User))
        {
            return;
        }

        _user = state.User;
        Changed?.Invoke();
    }

    private string? FindClaim(string type) => _user?.FindFirst(type)?.Value;
}
