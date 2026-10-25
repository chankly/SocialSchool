namespace HSoft.SocialSchool.Web.Models.Friends;

/// <summary>Dirección de una solicitud de amistad respecto al usuario de la sesión.</summary>
public enum FriendRequestDirection
{
    /// <summary>La solicitud la ha recibido el usuario actual.</summary>
    Incoming = 1,

    /// <summary>La solicitud la ha enviado el usuario actual.</summary>
    Outgoing = 2
}
