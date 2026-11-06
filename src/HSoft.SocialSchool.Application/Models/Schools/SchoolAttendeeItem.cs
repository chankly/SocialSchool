namespace HSoft.SocialSchool.Application.Models.Schools
{
    /// <summary>
    /// Proyección de lectura de un deportista con asistencia aceptada en una escuela. Cruza la asistencia
    /// con la cuenta —el nick— y con el perfil del deportista —la foto—, que viven en agregados distintos.
    /// </summary>
    /// <param name="Id">Identificador del deportista, que comparte con su cuenta.</param>
    /// <param name="Nickname">Nick del deportista, el único dato identificativo que es público.</param>
    /// <param name="ProfilePictureUrl">Foto del deportista, si la ha subido.</param>
    public sealed record SchoolAttendeeItem(Guid Id, string? Nickname, string? ProfilePictureUrl);
}
