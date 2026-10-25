namespace HSoft.SocialSchool.Application.Models.Profiles
{
    /// <summary>Datos del perfil de un deportista, tal y como los rellena el formulario de alta.</summary>
    /// <param name="FirstName">Nombre.</param>
    /// <param name="LastName">Apellidos.</param>
    /// <param name="DateOfBirth">Fecha de nacimiento; la edad se muestra calculada a partir de ella.</param>
    /// <param name="Country">País.</param>
    /// <param name="City">Ciudad.</param>
    /// <param name="ProfilePictureUrl">Foto de perfil, si la tiene.</param>
    /// <param name="Biography">Biografía breve, si la tiene.</param>
    /// <param name="PracticedSports">Deportes que practica, del catálogo.</param>
    public sealed record AthleteProfileResponse(
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        string Country,
        string City,
        string? ProfilePictureUrl,
        string? Biography,
        IReadOnlyList<SportResponse> PracticedSports);
}
