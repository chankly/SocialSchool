namespace HSoft.SocialSchool.Web.Services.Profile.Responses;

/// <summary>Deporte practicado incluido dentro del perfil de un deportista.</summary>
/// <param name="Id">Identificador del deporte.</param>
/// <param name="Name">Nombre del deporte.</param>
public sealed record SportSummary(Guid Id, string Name);
