namespace HSoft.SocialSchool.Web.Models.Common;

/// <summary>
/// Catálogo estático de países usados por los formularios de alta de perfil.
/// Se mantiene en el cliente mientras no exista un endpoint de referencia.
/// </summary>
public static class CountryCatalog
{
    /// <summary>Países disponibles en los desplegables.</summary>
    public static IReadOnlyList<Country> All { get; } =
    [
        new("ES", "España"),
        new("AR", "Argentina"),
        new("BO", "Bolivia"),
        new("BR", "Brasil"),
        new("CL", "Chile"),
        new("CO", "Colombia"),
        new("CR", "Costa Rica"),
        new("DO", "República Dominicana"),
        new("EC", "Ecuador"),
        new("FR", "Francia"),
        new("IT", "Italia"),
        new("MA", "Marruecos"),
        new("MX", "México"),
        new("NZ", "Nueva Zelanda"),
        new("PA", "Panamá"),
        new("PE", "Perú"),
        new("PT", "Portugal"),
        new("PY", "Paraguay"),
        new("GB", "Reino Unido"),
        new("UY", "Uruguay"),
        new("US", "Estados Unidos"),
        new("VE", "Venezuela")
    ];

    /// <summary>Comprueba si un código ISO está en el catálogo.</summary>
    public static bool Contains(string? code) =>
        !string.IsNullOrWhiteSpace(code)
        && All.Any(country => string.Equals(country.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>Devuelve el nombre del país a partir de su código ISO.</summary>
    public static string GetName(string? code)
    {
        var country = All.FirstOrDefault(item => string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));
        return country?.Name ?? (string.IsNullOrWhiteSpace(code) ? "Sin especificar" : code);
    }
}
