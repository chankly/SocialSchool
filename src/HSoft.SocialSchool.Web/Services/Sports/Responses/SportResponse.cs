namespace HSoft.SocialSchool.Web.Services.Sports.Responses;

/// <summary>Deporte del catálogo tal y como lo expondría la API.</summary>
/// <param name="Id">Identificador del deporte.</param>
/// <param name="Name">Nombre visible.</param>
/// <param name="Category">Familia a la que pertenece (agua, montaña, urbano…).</param>
public sealed record SportResponse(Guid Id, string Name, string Category);
