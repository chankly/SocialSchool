using CSharpFunctionalExtensions;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Friends
{
    /// <summary>
    /// Envía una solicitud de amistad a otra cuenta desde la del usuario autenticado. La solicitud nace
    /// pendiente y no crea la amistad hasta que el destinatario la acepta.
    /// </summary>
    /// <param name="UserId">Cuenta a la que se envía la solicitud.</param>
    public sealed record SendFriendRequestCommand(Guid UserId) : IRequest<Result<Guid>>;
}
