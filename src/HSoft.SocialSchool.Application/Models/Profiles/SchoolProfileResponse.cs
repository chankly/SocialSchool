namespace HSoft.SocialSchool.Application.Models.Profiles
{
    /// <summary>Datos del perfil de una escuela, tal y como los rellena el formulario de alta.</summary>
    /// <param name="Name">Nombre de la escuela.</param>
    /// <param name="LogoUrl">Logo, si lo tiene.</param>
    /// <param name="SportId">Deporte de la escuela; solo hay uno y se resuelve contra el catálogo.</param>
    /// <param name="Description">Descripción, si la tiene.</param>
    /// <param name="Country">País.</param>
    /// <param name="City">Ciudad.</param>
    /// <param name="ContactEmail">Correo de contacto, si lo tiene.</param>
    /// <param name="Phone">Teléfono, si lo tiene.</param>
    /// <param name="Website">Sitio web, si lo tiene.</param>
    public sealed record SchoolProfileResponse(
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
