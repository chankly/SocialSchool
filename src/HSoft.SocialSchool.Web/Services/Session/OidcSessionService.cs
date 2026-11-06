using System.Security.Claims;
using HSoft.SocialSchool.Web.Models.Accounts;
using HSoft.SocialSchool.Web.Services.Profile;
using Microsoft.AspNetCore.Components.Authorization;

namespace HSoft.SocialSchool.Web.Services.Session;

/// <summary>
/// Sesión respaldada por el token de Keycloak. La identidad se materializa una vez que
/// el proveedor de autenticación la resuelve; el tipo de cuenta se lee del claim
/// <c>userType</c> y se normaliza con <see cref="AccountTypeParser"/>. El estado de la
/// cuenta sale de <c>GET /api/user/profile/me</c>, que es la única fuente fiable.
/// </summary>
public sealed class OidcSessionService(
    AuthenticationStateProvider authenticationStateProvider,
    IProfileService profileService) : ISessionService
{
    private ClaimsPrincipal? _user;
    private AccountStatus? _accountStatus;
    private bool _accountStatusLoaded;

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
    public AccountStatus? AccountStatus => _accountStatus;

    /// <inheritdoc />
    public string? RawUserType => FindClaim("userType");

    /// <inheritdoc />
    public async Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        var changed = !ReferenceEquals(_user, state.User);

        _user = state.User;

        if (IsAuthenticated && !_accountStatusLoaded)
        {
            await LoadAccountStatusAsync(cancellationToken);
        }

        if (changed)
        {
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Consulta el perfil una sola vez para conocer el estado de la cuenta. Si falla, la
    /// cabecera se queda sin ese dato: la pantalla de perfil es la que explica el error.
    /// </summary>
    private async Task LoadAccountStatusAsync(CancellationToken cancellationToken)
    {
        _accountStatusLoaded = true;

        try
        {
            if (await profileService.GetMyProfileAsync(cancellationToken) is ProfileQueryResult.Success success)
            {
                _accountStatus = success.Profile.AccountStatus;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _accountStatusLoaded = false;
            throw;
        }
        catch (Exception)
        {
            _accountStatusLoaded = false;
        }
    }

    private string? FindClaim(string type) => _user?.FindFirst(type)?.Value;
}
