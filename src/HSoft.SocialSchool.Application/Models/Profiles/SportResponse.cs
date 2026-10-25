namespace HSoft.SocialSchool.Application.Models.Profiles
{
    /// <summary>Deporte del catálogo, reducido a lo que necesita un perfil.</summary>
    /// <param name="Id">Identificador del deporte.</param>
    /// <param name="Name">Nombre del deporte.</param>
    public sealed record SportResponse(Guid Id, string Name);
}
