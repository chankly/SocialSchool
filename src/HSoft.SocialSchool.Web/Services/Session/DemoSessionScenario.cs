namespace HSoft.SocialSchool.Web.Services.Session;

/// <summary>
/// Sesiones ficticias que permiten recorrer los estados de la maqueta sin Keycloak.
/// </summary>
public enum DemoSessionScenario
{
    /// <summary>Deportista con perfil: la consulta responde 200.</summary>
    AthleteWithProfile = 1,

    /// <summary>Deportista autenticado sin perfil: la consulta responde 204.</summary>
    AthleteWithoutProfile = 2,

    /// <summary>Escuela validada con perfil: la consulta responde 200.</summary>
    SchoolWithProfile = 3,

    /// <summary>Escuela pendiente de validación y sin perfil: la consulta responde 204.</summary>
    SchoolWithoutProfile = 4,

    /// <summary>Sin token: la consulta responde 401.</summary>
    SignedOut = 5
}
