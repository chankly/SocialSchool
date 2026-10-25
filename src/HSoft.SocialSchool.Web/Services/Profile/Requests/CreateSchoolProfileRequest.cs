namespace HSoft.SocialSchool.Web.Services.Profile.Requests;

/// <summary>
/// Datos que envía el formulario de alta de una escuela. El nick se fija aquí y ya no
/// se puede modificar después.
/// </summary>
public sealed class CreateSchoolProfileRequest
{
    /// <summary>Nick único, inmutable y solo alfanumérico.</summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>Nombre de la escuela.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL pública del logo.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Deporte principal de la escuela. Solo se admite uno.</summary>
    public Guid SportId { get; set; }

    /// <summary>Descripción de la escuela.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Código ISO del país.</summary>
    public string Country { get; set; } = "ES";

    /// <summary>Ciudad.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Correo de contacto público.</summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>Teléfono de contacto público.</summary>
    public string? Phone { get; set; }

    /// <summary>Sitio web de la escuela.</summary>
    public string? Website { get; set; }
}
