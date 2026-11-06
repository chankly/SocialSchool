using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Profiles;
using HSoft.SocialSchool.Application.Repositories;
using Mediator;

namespace HSoft.SocialSchool.Application.Queries.Sports
{
    /// <summary>
    /// Devuelve los deportes activos del catálogo con su identificador y su nombre. La entidad de dominio no
    /// sale de la capa de aplicación.
    /// </summary>
    public sealed class GetSportsQueryHandler
        : IRequestHandler<GetSportsQuery, Result<IReadOnlyList<SportResponse>>>
    {
        private readonly ISportRepository _sportRepository;

        public GetSportsQueryHandler(ISportRepository sportRepository)
        {
            _sportRepository = sportRepository;
        }

        public async ValueTask<Result<IReadOnlyList<SportResponse>>> Handle(
            GetSportsQuery request,
            CancellationToken cancellationToken)
        {
            var sports = await _sportRepository.GetActiveReadOnlyAsync(cancellationToken);

            var response = sports
                .Select(sport => new SportResponse(sport.Id, sport.Name))
                .ToList();

            return Result.Success<IReadOnlyList<SportResponse>>(response);
        }
    }
}
