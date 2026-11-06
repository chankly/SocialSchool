using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Schools;
using Mediator;

namespace HSoft.SocialSchool.Application.Queries.Schools
{
    /// <summary>
    /// Consulta el directorio de escuelas. Los tres filtros son opcionales y se pueden combinar; sin
    /// ninguno se devuelve el directorio completo, ordenado por nombre.
    /// </summary>
    /// <param name="SportId">Deporte por el que filtrar, si se indica.</param>
    /// <param name="City">Ciudad por la que filtrar, si se indica.</param>
    /// <param name="Search">Texto que debe contener el nombre, si se indica.</param>
    public sealed record GetSchoolsQuery(Guid? SportId, string? City, string? Search)
        : IRequest<Result<IReadOnlyList<SchoolSummaryResponse>>>;
}
