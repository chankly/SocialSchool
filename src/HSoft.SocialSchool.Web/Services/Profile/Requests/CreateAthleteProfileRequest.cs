namespace HSoft.SocialSchool.Web.Services.Profile.Requests;

/// <summary>
/// Datos que envía el formulario de alta de un deportista. El nick se fija aquí y ya no
/// se puede modificar después; la fecha se maneja como <see cref="DateTime"/> porque es
/// el tipo que mejor encaja con el input de fecha del formulario.
/// </summary>
public sealed class CreateAthleteProfileRequest
{
    /// <summary>Nick único, inmutable y solo alfanumérico.</summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>Nombre del deportista.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Apellidos del deportista.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Fecha de nacimiento.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Código ISO del país de residencia.</summary>
    public string Country { get; set; } = "ES";

    /// <summary>Ciudad de residencia.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>URL pública de la foto de perfil.</summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>Biografía breve.</summary>
    public string? Biography { get; set; }

    /// <summary>Deportes que practica.</summary>
    public List<Guid> PracticedSportIds { get; set; } = [];
}
