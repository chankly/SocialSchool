using HSoft.SocialSchool.Web.Services.Demo;
using HSoft.SocialSchool.Web.Services.Schools.Responses;
using HSoft.SocialSchool.Web.Services.Sports;

namespace HSoft.SocialSchool.Web.Services.Schools;

/// <summary>
/// Catálogo simulado de escuelas. El perfil público incluye servicios y asistentes;
/// el perfil propio, en cambio, no los devuelve, tal y como fija el contrato de perfil.
/// </summary>
public sealed class MockSchoolsService : ISchoolsService
{
    private static readonly IReadOnlyList<SchoolDetailResponse> Schools = BuildSchools();

    /// <inheritdoc />
    public async Task<IReadOnlyList<SchoolListItemResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(260, cancellationToken);
        return Schools
            .Select(detail => detail.School)
            .OrderBy(school => school.Name)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<SchoolDetailResponse?> GetByIdAsync(Guid schoolId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(220, cancellationToken);
        return Schools.FirstOrDefault(detail => detail.School.Id == schoolId);
    }

    private static IReadOnlyList<SchoolDetailResponse> BuildSchools() =>
    [
        new SchoolDetailResponse(
            new SchoolListItemResponse(
                DemoIds.SchoolWithProfile,
                "Olas del Norte",
                "olasdelnorte",
                SportCatalog.SurfId,
                "Surf",
                "San Sebastián",
                "ES",
                "Escuela de surf familiar en la playa de Zurriola. Clases de iniciación, perfeccionamiento y surf adaptado durante todo el año.",
                null,
                4,
                true),
            "hola@olasdelnorte.example",
            "+34 943 555 010",
            "https://olasdelnorte.example",
            [
                new SchoolServiceResponse(
                    Guid.Parse("11110000-0000-4000-8000-000000000001"),
                    "Clase de iniciación",
                    "Grupos de seis personas con material incluido. Tabla blanda y neopreno.",
                    35m,
                    "Sábados · 09:00–11:00"),
                new SchoolServiceResponse(
                    Guid.Parse("11110000-0000-4000-8000-000000000002"),
                    "Perfeccionamiento en grupo",
                    "Lectura de olas, prioridad y maniobras básicas para quien ya se levanta.",
                    45m,
                    "Martes y jueves · 18:00"),
                new SchoolServiceResponse(
                    Guid.Parse("11110000-0000-4000-8000-000000000003"),
                    "Surf adaptado",
                    "Sesiones individuales con material adaptado y monitor especializado.",
                    40m,
                    "Con cita previa")
            ],
            [
                new SchoolAttendeeResponse(DemoIds.MartaElorza, "Marta Elorza", "Surf · 3 años en la escuela"),
                new SchoolAttendeeResponse(DemoIds.IkerLanda, "Iker Landa", "Skateboarding · asistencia puntual"),
                new SchoolAttendeeResponse(DemoIds.NereaEtxeberria, "Nerea Etxeberria", "Surf · 1 año en la escuela"),
                new SchoolAttendeeResponse(DemoIds.AneZubia, "Ane Zubia", "Surf · campamentos de verano")
            ]),

        new SchoolDetailResponse(
            new SchoolListItemResponse(
                DemoIds.ZarautzWaves,
                "Zarautz Waves",
                "zarautzwaves",
                SportCatalog.SurfId,
                "Surf",
                "Zarautz",
                "ES",
                "Campamentos de surf y clases sueltas en la playa de Zarautz. Ambiente tranquilo y grupos reducidos.",
                null,
                3,
                true),
            "info@zarautzwaves.example",
            "+34 943 555 021",
            "https://zarautzwaves.example",
            [
                new SchoolServiceResponse(
                    Guid.Parse("22220000-0000-4000-8000-000000000001"),
                    "Clase suelta",
                    "Dos horas de clase con tabla y neopreno incluidos.",
                    30m,
                    "Todos los días · 10:00"),
                new SchoolServiceResponse(
                    Guid.Parse("22220000-0000-4000-8000-000000000002"),
                    "Campamento semanal",
                    "Cinco mañanas de surf, almuerzo y actividades de tarde.",
                    210m,
                    "Lunes a viernes · julio y agosto")
            ],
            [
                new SchoolAttendeeResponse(DemoIds.JonAranburu, "Jon Aranburu", "Surf · 5 años en la escuela"),
                new SchoolAttendeeResponse(DemoIds.LucasFerreira, "Lucas Ferreira", "Skateboarding · verano"),
                new SchoolAttendeeResponse(DemoIds.AneZubia, "Ane Zubia", "Surf · clases sueltas")
            ]),

        new SchoolDetailResponse(
            new SchoolListItemResponse(
                DemoIds.BilbaoSkateLab,
                "Bilbao Skate Lab",
                "bilbaoskatelab",
                SportCatalog.SkateboardingId,
                "Skateboarding",
                "Bilbao",
                "ES",
                "Skatepark cubierto y clases para todos los niveles. Ramplas, bowl y zona de iniciación.",
                null,
                2,
                true),
            "hola@bilbaoskatelab.example",
            "+34 944 555 030",
            "https://bilbaoskatelab.example",
            [
                new SchoolServiceResponse(
                    Guid.Parse("33330000-0000-4000-8000-000000000001"),
                    "Clase grupal",
                    "Técnica básica, seguridad y primeras líneas en el bowl.",
                    25m,
                    "Miércoles y viernes · 17:30"),
                new SchoolServiceResponse(
                    Guid.Parse("33330000-0000-4000-8000-000000000002"),
                    "Sesión libre",
                    "Acceso al skatepark cubierto durante dos horas.",
                    8m,
                    "Todos los días · 16:00–22:00")
            ],
            [
                new SchoolAttendeeResponse(DemoIds.IkerLanda, "Iker Landa", "Skateboarding · 2 años en la escuela"),
                new SchoolAttendeeResponse(DemoIds.LucasFerreira, "Lucas Ferreira", "Skateboarding · clases grupales")
            ]),

        new SchoolDetailResponse(
            new SchoolListItemResponse(
                DemoIds.DonostiaClimbClub,
                "Donostia Climb Club",
                "donostiaclimb",
                SportCatalog.EscaladaId,
                "Escalada",
                "San Sebastián",
                "ES",
                "Rocódromo y salidas a vía en la costa. Escuela para adultos y campus de verano para jóvenes.",
                null,
                2,
                true),
            "contacto@donostiaclimb.example",
            "+34 943 555 041",
            "https://donostiaclimb.example",
            [
                new SchoolServiceResponse(
                    Guid.Parse("44440000-0000-4000-8000-000000000001"),
                    "Bono mensual",
                    "Acceso libre al rocódromo y a las clases colectivas de técnica.",
                    55m,
                    "Todos los días · 08:00–23:00"),
                new SchoolServiceResponse(
                    Guid.Parse("44440000-0000-4000-8000-000000000002"),
                    "Clase de técnica",
                    "Trabajo de pies, lectura de rutas y seguridad en bloque.",
                    30m,
                    "Lunes y jueves · 19:00")
            ],
            [
                new SchoolAttendeeResponse(DemoIds.MartaElorza, "Marta Elorza", "Escalada · bono mensual"),
                new SchoolAttendeeResponse(DemoIds.JonAranburu, "Jon Aranburu", "Escalada · clases de técnica")
            ]),

        new SchoolDetailResponse(
            new SchoolListItemResponse(
                DemoIds.MundakaSurfHouse,
                "Mundaka Surf House",
                "mundakasurf",
                SportCatalog.SurfId,
                "Surf",
                "Mundaka",
                "ES",
                "Casa de surf y escuela en la reserva de Urdaibai. Estancia, clases y salidas en barco a la ola.",
                null,
                2,
                true),
            "reservas@mundakasurf.example",
            "+34 946 555 052",
            "https://mundakasurf.example",
            [
                new SchoolServiceResponse(
                    Guid.Parse("55550000-0000-4000-8000-000000000001"),
                    "Clase de longboard",
                    "Estilo clásico, cross-step y lectura de la ola de Mundaka.",
                    35m,
                    "Viernes y sábados · 09:30"),
                new SchoolServiceResponse(
                    Guid.Parse("55550000-0000-4000-8000-000000000002"),
                    "Pack fin de semana",
                    "Dos noches de alojamiento y dos clases de surf.",
                    90m,
                    "Viernes a domingo")
            ],
            [
                new SchoolAttendeeResponse(DemoIds.JonAranburu, "Jon Aranburu", "Surf · 4 años en la escuela"),
                new SchoolAttendeeResponse(DemoIds.MartaElorza, "Marta Elorza", "Surf · salidas de fin de semana")
            ]),

        new SchoolDetailResponse(
            new SchoolListItemResponse(
                DemoIds.SierraNorteAventura,
                "Sierra Norte Aventura",
                "sierranorte",
                SportCatalog.EscaladaId,
                "Escalada",
                "Madrid",
                "ES",
                "Escuela de escalada en La Pedriza y Patones. Cursos de iniciación, vías y boulder.",
                null,
                1,
                true),
            "info@sierranorte.example",
            "+34 918 555 060",
            "https://sierranorte.example",
            [
                new SchoolServiceResponse(
                    Guid.Parse("66660000-0000-4000-8000-000000000001"),
                    "Curso de iniciación",
                    "Dos fines de semana con material y seguro incluidos.",
                    120m,
                    "Sábados y domingos · 09:00"),
                new SchoolServiceResponse(
                    Guid.Parse("66660000-0000-4000-8000-000000000002"),
                    "Salida guiada",
                    "Jornada de vías en La Pedriza con guía titulado.",
                    45m,
                    "Domingos · 08:00")
            ],
            [
                new SchoolAttendeeResponse(DemoIds.LucasFerreira, "Lucas Ferreira", "Escalada · curso de iniciación")
            ])
    ];
}
