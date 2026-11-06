namespace HSoft.SocialSchool.Application.Models.Profiles
{
    /// <summary>
    /// Datos del formulario de alta de un deportista. Llegan en el bloque <c>athlete</c> de la petición.
    /// </summary>
    /// <param name="FirstName">Nombre.</param>
    /// <param name="LastName">Apellidos.</param>
    /// <param name="DateOfBirth">Fecha de nacimiento; tiene que acreditar la mayoría de edad.</param>
    /// <param name="Country">País.</param>
    /// <param name="City">Ciudad.</param>
    /// <param name="ProfilePictureUrl">Foto de perfil, si la tiene.</param>
    /// <param name="Biography">Biografía breve, si la tiene.</param>
    /// <param name="PracticedSportIds">Deportes que practica, por identificador del catálogo.</param>
    public sealed record AthleteProfileRequest(
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        string Country,
        string City,
        string? ProfilePictureUrl,
        string? Biography,
        IReadOnlyList<Guid>? PracticedSportIds);
}
