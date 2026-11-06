namespace HSoft.SocialSchool.Application.Models.Friends
{
    /// <summary>
    /// Solicitud de amistad enviada por el usuario autenticado y todavía pendiente, que puede cancelar.
    /// </summary>
    /// <param name="Id">Identificador de la solicitud.</param>
    /// <param name="Receiver">Cuenta a la que se envió la solicitud.</param>
    /// <param name="RequestedAt">Momento en el que se envió la solicitud.</param>
    public sealed record OutgoingFriendRequestResponse(
        Guid Id,
        FriendAccountResponse Receiver,
        DateTime RequestedAt);
}
