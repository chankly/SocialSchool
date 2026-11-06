using HSoft.SocialSchool.Domain.Enumerations;

namespace HSoft.SocialSchool.Application.Models.Schools
{
    /// <summary>
    /// Ficha pública de una escuela: sus datos de contacto, los servicios que ofrece y los deportistas que
    /// han asistido con la asistencia aceptada.
    /// </summary>
    /// <param name="Id">Identificador de la escuela, que comparte con su cuenta.</param>
    /// <param name="Name">Nombre público de la escuela.</param>
    /// <param name="LogoUrl">Logo de la escuela, si lo ha subido.</param>
    /// <param name="SportId">Identificador del deporte que practica.</param>
    /// <param name="SportName">Nombre del deporte que practica.</param>
    /// <param name="Description">Descripción de la escuela, si la ha escrito.</param>
    /// <param name="Country">País de la escuela.</param>
    /// <param name="City">Ciudad de la escuela.</param>
    /// <param name="ContactEmail">Correo de contacto, si lo ha publicado.</param>
    /// <param name="Phone">Teléfono de contacto, si lo ha publicado.</param>
    /// <param name="Website">Sitio web, si lo ha publicado.</param>
    /// <param name="AccountStatus">Estado de la cuenta; puede estar pendiente de validación.</param>
    /// <param name="Services">Servicios que ofrece la escuela.</param>
    /// <param name="Attendees">Deportistas con asistencia aceptada, con su nick y su foto.</param>
    public sealed record SchoolDetailResponse(
        Guid Id,
        string Name,
        string? LogoUrl,
        Guid SportId,
        string SportName,
        string? Description,
        string Country,
        string City,
        string? ContactEmail,
        string? Phone,
        string? Website,
        AccountStatus AccountStatus,
        IReadOnlyList<SchoolServiceResponse> Services,
        IReadOnlyList<SchoolAttendeeResponse> Attendees);
}
