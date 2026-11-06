using HSoft.SocialSchool.Domain.Enumerations;

namespace HSoft.SocialSchool.Application.Models.Schools
{
    /// <summary>
    /// Escuela tal y como aparece en el directorio. Lleva el estado de la cuenta para que el cliente pueda
    /// señalar que todavía está pendiente de validación.
    /// </summary>
    /// <param name="Id">Identificador de la escuela, que comparte con su cuenta.</param>
    /// <param name="Name">Nombre público de la escuela.</param>
    /// <param name="LogoUrl">Logo de la escuela, si lo ha subido.</param>
    /// <param name="SportId">Identificador del deporte que practica.</param>
    /// <param name="SportName">Nombre del deporte que practica.</param>
    /// <param name="City">Ciudad de la escuela.</param>
    /// <param name="AccountStatus">Estado de la cuenta; puede estar pendiente de validación.</param>
    public sealed record SchoolSummaryResponse(
        Guid Id,
        string Name,
        string? LogoUrl,
        Guid SportId,
        string SportName,
        string City,
        AccountStatus AccountStatus);
}
