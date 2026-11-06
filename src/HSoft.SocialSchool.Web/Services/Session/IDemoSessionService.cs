namespace HSoft.SocialSchool.Web.Services.Session;

/// <summary>
/// Sesión de demostración: permite alternar los escenarios del boceto sin Keycloak.
/// Solo se registra en modo demo; el resto de la aplicación depende únicamente de
/// <see cref="ISessionService"/>.
/// </summary>
public interface IDemoSessionService : ISessionService
{
    /// <summary>Escenario de demostración activo.</summary>
    DemoSessionScenario Scenario { get; }

    /// <summary>Cambia de escenario y notifica a los componentes suscritos.</summary>
    void UseScenario(DemoSessionScenario scenario);

    /// <summary>Vuelve al escenario inicial de la maqueta.</summary>
    void Reset();
}
