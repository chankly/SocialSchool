using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Profiles;
using Mediator;

namespace HSoft.SocialSchool.Application.Queries.Sports
{
    /// <summary>
    /// Consulta el catálogo de deportes activos para el selector del formulario de perfil.
    /// </summary>
    public sealed record GetSportsQuery : IRequest<Result<IReadOnlyList<SportResponse>>>;
}
