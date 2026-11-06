using HSoft.SocialSchool.Domain.Enumerations;

namespace HSoft.SocialSchool.Application.Models.Friends
{
    /// <summary>
    /// Cuenta a la que el usuario autenticado puede enviar una solicitud de amistad. No incluye a sus
    /// amigos ni a quien ya tiene una solicitud pendiente con él.
    /// </summary>
    /// <param name="Id">Identificador de la cuenta.</param>
    /// <param name="Nickname">Nick de la cuenta.</param>
    /// <param name="AccountType">Tipo de la cuenta: deportista o escuela.</param>
    /// <param name="City">Ciudad del perfil de la cuenta, si la ha rellenado.</param>
    public sealed record DiscoverableAccountResponse(
        Guid Id,
        string? Nickname,
        AccountType AccountType,
        string? City);
}
