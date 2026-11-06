using CSharpFunctionalExtensions;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Friends
{
    /// <summary>
    /// Deshace la amistad con otra cuenta. Cualquiera de las dos partes puede hacerlo y la amistad
    /// desaparece para las dos.
    /// </summary>
    /// <param name="UserId">La otra cuenta de la amistad.</param>
    public sealed record RemoveFriendCommand(Guid UserId) : IRequest<Result>;
}
