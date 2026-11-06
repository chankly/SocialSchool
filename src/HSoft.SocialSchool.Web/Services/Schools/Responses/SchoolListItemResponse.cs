namespace HSoft.SocialSchool.Web.Services.Schools.Responses;

/// <summary>Escuela tal y como aparece en el listado público.</summary>
public sealed record SchoolListItemResponse(
    Guid Id,
    string Name,
    string? LogoUrl,
    Guid SportId,
    string SportName,
    string City);
