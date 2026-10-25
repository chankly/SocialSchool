namespace HSoft.SocialSchool.Web.Services.Friends;

/// <summary>Resultado de una operación sobre amistades o solicitudes.</summary>
public sealed record FriendActionResult(bool IsSuccess, string? Error)
{
    /// <summary>La operación se ha aplicado en el estado simulado.</summary>
    public static FriendActionResult Ok() => new(true, null);

    /// <summary>La operación se ha rechazado con un mensaje para el usuario.</summary>
    public static FriendActionResult Fail(string error) => new(false, error);
}
