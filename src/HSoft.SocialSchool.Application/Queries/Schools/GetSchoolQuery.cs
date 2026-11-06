using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Schools;
using Mediator;

namespace HSoft.SocialSchool.Application.Queries.Schools
{
    /// <summary>
    /// Consulta la ficha pública de una escuela. El valor es <see langword="null"/> cuando el identificador
    /// no corresponde a ninguna escuela, para que la API responda <c>404</c>.
    /// </summary>
    /// <param name="Id">Identificador de la escuela, que comparte con su cuenta.</param>
    public sealed record GetSchoolQuery(Guid Id) : IRequest<Result<SchoolDetailResponse?>>;
}
