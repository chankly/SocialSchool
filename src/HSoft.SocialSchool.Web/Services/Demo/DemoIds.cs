namespace HSoft.SocialSchool.Web.Services.Demo;

/// <summary>
/// Identificadores fijos de las cuentas ficticias. Compartirlos entre los mocks permite
/// que una escuela del catálogo sea la misma cuenta que aparece en amistades.
/// </summary>
public static class DemoIds
{
    // Cuentas que se pueden elegir como sesión.
    public static readonly Guid AthleteWithProfile = Guid.Parse("aaaaaaaa-0000-4000-8000-000000000001");
    public static readonly Guid AthleteWithoutProfile = Guid.Parse("aaaaaaaa-0000-4000-8000-000000000002");
    public static readonly Guid SchoolWithProfile = Guid.Parse("bbbbbbbb-0000-4000-8000-000000000003");
    public static readonly Guid SchoolWithoutProfile = Guid.Parse("bbbbbbbb-0000-4000-8000-000000000004");

    // Personas que aparecen en amistades.
    public static readonly Guid MartaElorza = Guid.Parse("dddddddd-0000-4000-8000-000000000020");
    public static readonly Guid IkerLanda = Guid.Parse("dddddddd-0000-4000-8000-000000000021");
    public static readonly Guid NereaEtxeberria = Guid.Parse("dddddddd-0000-4000-8000-000000000022");
    public static readonly Guid JonAranburu = Guid.Parse("dddddddd-0000-4000-8000-000000000023");
    public static readonly Guid AneZubia = Guid.Parse("dddddddd-0000-4000-8000-000000000024");
    public static readonly Guid LucasFerreira = Guid.Parse("dddddddd-0000-4000-8000-000000000025");

    // Escuelas del catálogo.
    public static readonly Guid ZarautzWaves = Guid.Parse("eeeeeeee-0000-4000-8000-000000000010");
    public static readonly Guid BilbaoSkateLab = Guid.Parse("eeeeeeee-0000-4000-8000-000000000011");
    public static readonly Guid DonostiaClimbClub = Guid.Parse("eeeeeeee-0000-4000-8000-000000000012");
    public static readonly Guid MundakaSurfHouse = Guid.Parse("eeeeeeee-0000-4000-8000-000000000013");
    public static readonly Guid SierraNorteAventura = Guid.Parse("eeeeeeee-0000-4000-8000-000000000014");

    // Solicitudes de amistad.
    public static readonly Guid IncomingRequestFromNerea = Guid.Parse("cccccccc-0000-4000-8000-000000000001");
    public static readonly Guid IncomingRequestFromBilbao = Guid.Parse("cccccccc-0000-4000-8000-000000000002");
    public static readonly Guid OutgoingRequestToJon = Guid.Parse("cccccccc-0000-4000-8000-000000000003");
}
