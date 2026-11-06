namespace HSoft.SocialSchool.Application.Models.Schools
{
    /// <summary>
    /// Deportista que ha asistido a una escuela. Solo se publica el nick y la foto: el resto del perfil es
    /// privado para quien no es amigo ni escuela con asistencia aceptada.
    /// </summary>
    /// <param name="Id">Identificador del deportista, que comparte con su cuenta.</param>
    /// <param name="Nickname">Nick del deportista.</param>
    /// <param name="ProfilePictureUrl">Foto del deportista, si la ha subido.</param>
    public sealed record SchoolAttendeeResponse(Guid Id, string? Nickname, string? ProfilePictureUrl);
}
