using HSoft.SocialSchool.Web.Services.Sports.Responses;

namespace HSoft.SocialSchool.Web.Services.Sports;

/// <summary>
/// Catálogo ficticio de deportes. Los identificadores son fijos para que los perfiles,
/// las escuelas y los formularios se refieran siempre al mismo deporte.
/// </summary>
public static class SportCatalog
{
    /// <summary>Identificador del surf.</summary>
    public static readonly Guid SurfId = Guid.Parse("5f000000-0000-4000-8000-000000000001");

    /// <summary>Identificador del skateboarding.</summary>
    public static readonly Guid SkateboardingId = Guid.Parse("5f000000-0000-4000-8000-000000000002");

    /// <summary>Identificador del snowboard.</summary>
    public static readonly Guid SnowboardId = Guid.Parse("5f000000-0000-4000-8000-000000000003");

    /// <summary>Identificador del kitesurf.</summary>
    public static readonly Guid KitesurfId = Guid.Parse("5f000000-0000-4000-8000-000000000004");

    /// <summary>Identificador del wakeboard.</summary>
    public static readonly Guid WakeboardId = Guid.Parse("5f000000-0000-4000-8000-000000000005");

    /// <summary>Identificador del bodyboard.</summary>
    public static readonly Guid BodyboardId = Guid.Parse("5f000000-0000-4000-8000-000000000006");

    /// <summary>Identificador del longboard.</summary>
    public static readonly Guid LongboardId = Guid.Parse("5f000000-0000-4000-8000-000000000007");

    /// <summary>Identificador de la escalada.</summary>
    public static readonly Guid EscaladaId = Guid.Parse("5f000000-0000-4000-8000-000000000008");

    /// <summary>Identificador del BMX.</summary>
    public static readonly Guid BmxId = Guid.Parse("5f000000-0000-4000-8000-000000000009");

    /// <summary>Identificador del windsurf.</summary>
    public static readonly Guid WindsurfId = Guid.Parse("5f000000-0000-4000-8000-000000000010");

    /// <summary>Todos los deportes disponibles.</summary>
    public static IReadOnlyList<SportResponse> All { get; } =
    [
        new(SurfId, "Surf", "Agua"),
        new(SkateboardingId, "Skateboarding", "Urbano"),
        new(SnowboardId, "Snowboard", "Montaña"),
        new(KitesurfId, "Kitesurf", "Agua"),
        new(WakeboardId, "Wakeboard", "Agua"),
        new(BodyboardId, "Bodyboard", "Agua"),
        new(LongboardId, "Longboard", "Urbano"),
        new(EscaladaId, "Escalada", "Montaña"),
        new(BmxId, "BMX", "Urbano"),
        new(WindsurfId, "Windsurf", "Agua")
    ];

    /// <summary>Busca un deporte por identificador.</summary>
    public static SportResponse? Find(Guid sportId) => All.FirstOrDefault(sport => sport.Id == sportId);

    /// <summary>Devuelve el nombre del deporte o un texto de respaldo.</summary>
    public static string GetName(Guid sportId) => Find(sportId)?.Name ?? "Deporte no disponible";
}
