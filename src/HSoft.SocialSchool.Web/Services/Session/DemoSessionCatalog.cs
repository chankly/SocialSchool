namespace HSoft.SocialSchool.Web.Services.Session;

/// <summary>Opción del selector de sesión ficticia de la cabecera.</summary>
/// <param name="Scenario">Escenario que se simula.</param>
/// <param name="Label">Texto corto del desplegable.</param>
/// <param name="Detail">Descripción mostrada en la ayuda.</param>
/// <param name="RawUserType">Valor que traería el atributo <c>userType</c> del token.</param>
public sealed record DemoSessionOption(
    DemoSessionScenario Scenario,
    string Label,
    string Detail,
    string? RawUserType);

/// <summary>
/// Escenarios disponibles en el selector de sesión. Se incluyen valores en inglés y en
/// español para comprobar la normalización del tipo de cuenta que hace el cliente.
/// </summary>
public static class DemoSessionCatalog
{
    /// <summary>Todas las sesiones de demostración.</summary>
    public static IReadOnlyList<DemoSessionOption> All { get; } =
    [
        new(
            DemoSessionScenario.AthleteWithProfile,
            "Deportista · con perfil",
            "Ana Otxoa ya ha completado su perfil.",
            "Deportista"),
        new(
            DemoSessionScenario.AthleteWithoutProfile,
            "Deportista · sin perfil",
            "Cuenta recién registrada a la que le falta el formulario de alta.",
            "Athlete"),
        new(
            DemoSessionScenario.SchoolWithProfile,
            "Escuela · con perfil",
            "Olas del Norte, una escuela ya validada.",
            "School"),
        new(
            DemoSessionScenario.SchoolWithoutProfile,
            "Escuela · sin perfil",
            "Escuela pendiente de validación que todavía no ha rellenado su perfil.",
            "Escuela"),
        new(
            DemoSessionScenario.SignedOut,
            "Sin sesión (401)",
            "No hay token válido: la web pediría autenticarse en Keycloak.",
            null)
    ];

    /// <summary>Obtiene la opción asociada a un escenario.</summary>
    public static DemoSessionOption For(DemoSessionScenario scenario) =>
        All.First(option => option.Scenario == scenario);
}
