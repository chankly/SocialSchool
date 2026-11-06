namespace HSoft.SocialSchool.Application.Models.Friends
{
    /// <summary>
    /// Confirmación del envío de una solicitud de amistad: el cliente recibe su identificador para poder
    /// aceptarla, rechazarla o cancelarla.
    /// </summary>
    /// <param name="Id">Identificador de la solicitud creada.</param>
    public sealed record FriendRequestCreatedResponse(Guid Id);
}
