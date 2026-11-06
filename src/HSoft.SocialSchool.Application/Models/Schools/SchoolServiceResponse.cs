namespace HSoft.SocialSchool.Application.Models.Schools
{
    /// <summary>
    /// Servicio que ofrece una escuela. Es informativo: desde la aplicación no se contrata ni se paga.
    /// </summary>
    /// <param name="Id">Identificador del servicio.</param>
    /// <param name="Name">Nombre del servicio.</param>
    /// <param name="Description">Descripción del servicio, si la tiene.</param>
    /// <param name="Price">Precio orientativo del servicio.</param>
    /// <param name="Currency">Moneda del precio, en formato ISO de tres letras.</param>
    public sealed record SchoolServiceResponse(
        Guid Id,
        string Name,
        string? Description,
        decimal Price,
        string Currency);
}
