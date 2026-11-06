using HSoft.SocialSchool.Domain.Enumerations;

namespace HSoft.SocialSchool.Application.Models.Friends
{
    /// <summary>
    /// Cuenta que aparece en las listas de amistades: un amigo o la otra parte de una solicitud.
    /// </summary>
    /// <param name="Id">Identificador de la cuenta.</param>
    /// <param name="Nickname">Nick de la cuenta, que es su dato público.</param>
    /// <param name="AccountType">Tipo de la cuenta: deportista o escuela.</param>
    public sealed record FriendAccountResponse(Guid Id, string? Nickname, AccountType AccountType);
}
