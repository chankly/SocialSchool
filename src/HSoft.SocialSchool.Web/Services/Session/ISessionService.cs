using HSoft.SocialSchool.Web.Models.Accounts;

namespace HSoft.SocialSchool.Web.Services.Session;

/// <summary>
/// Identidad del usuario autenticado, resuelta a partir de los claims del token de
/// Keycloak. El tipo de cuenta llega como <c>userType</c> y se normaliza con
/// <see cref="AccountTypeParser"/>.
/// </summary>
public interface ISessionService
{
    /// <summary>Se dispara cuando cambia la sesión.</summary>
    event Action? Changed;

    /// <summary>Indica si hay una sesión válida. Sin sesión, la API respondería 401.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Identificador de la cuenta (equivalente al <c>sub</c> del token).</summary>
    Guid UserId { get; }

    /// <summary>Correo de la cuenta.</summary>
    string Email { get; }

    /// <summary>Nombre visible de la cuenta.</summary>
    string DisplayName { get; }

    /// <summary>Tipo de cuenta ya normalizado, o <see langword="null"/> si no se reconoce.</summary>
    AccountType? AccountType { get; }

    /// <summary>Estado de la cuenta, o <see langword="null"/> si no hay sesión.</summary>
    AccountStatus? AccountStatus { get; }

    /// <summary>Valor bruto del token, tal y como llegaría desde Keycloak.</summary>
    string? RawUserType { get; }

    /// <summary>
    /// Resuelve la identidad cuando el proveedor la carga de forma asíncrona (OIDC).
    /// </summary>
    Task EnsureLoadedAsync(CancellationToken cancellationToken = default);
}
