namespace HSoft.SocialSchool.Application.Models.Profiles
{
    /// <summary>
    /// Datos del formulario de alta de una escuela. Llegan en el bloque <c>school</c> de la petición.
    /// </summary>
    /// <param name="Name">Nombre de la escuela.</param>
    /// <param name="LogoUrl">Logo, si lo tiene.</param>
    /// <param name="SportId">Deporte de la escuela, que es único y se fija al crearla.</param>
    /// <param name="Description">Descripción, si la tiene.</param>
    /// <param name="Country">País.</param>
    /// <param name="City">Ciudad.</param>
    /// <param name="ContactEmail">Correo de contacto, si lo tiene.</param>
    /// <param name="Phone">Teléfono, si lo tiene.</param>
    /// <param name="Website">Sitio web, si lo tiene.</param>
    public sealed record SchoolProfileRequest(
        string Name,
        string? LogoUrl,
        Guid SportId,
        string? Description,
        string Country,
        string City,
        string? ContactEmail,
        string? Phone,
        string? Website);
}
