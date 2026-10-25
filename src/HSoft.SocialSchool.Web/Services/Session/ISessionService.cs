using HSoft.SocialSchool.Web.Models.Accounts;

namespace HSoft.SocialSchool.Web.Services.Session;

/// <summary>
/// Identidad del usuario en la maqueta. Sustituye a la autenticación real contra Keycloak:
/// el tipo de cuenta se obtiene de un valor equivalente al claim <c>userType</c> del token
/// y se normaliza con <see cref="AccountTypeParser"/>.
/// </summary>
public interface ISessionService
{
    /// <summary>Se dispara cuando cambia la sesión simulada.</summary>
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

    /// <summary>Escenario de demostración activo.</summary>
    DemoSessionScenario Scenario { get; }

    /// <summary>Cambia de escenario y notifica a los componentes suscritos.</summary>
    void UseScenario(DemoSessionScenario scenario);

    /// <summary>Vuelve al escenario inicial de la maqueta.</summary>
    void Reset();
}
