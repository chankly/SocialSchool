using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Schools;
using HSoft.SocialSchool.Application.Repositories;
using Mediator;

namespace HSoft.SocialSchool.Application.Queries.Schools
{
    /// <summary>
    /// Devuelve el directorio de escuelas. El filtrado y el orden los resuelve la base de datos, cuya
    /// colación no distingue mayúsculas; aquí solo se proyecta al contrato de la API.
    /// </summary>
    public sealed class GetSchoolsQueryHandler
        : IRequestHandler<GetSchoolsQuery, Result<IReadOnlyList<SchoolSummaryResponse>>>
    {
        private readonly ISchoolRepository _schoolRepository;

        public GetSchoolsQueryHandler(ISchoolRepository schoolRepository)
        {
            _schoolRepository = schoolRepository;
        }

        public async ValueTask<Result<IReadOnlyList<SchoolSummaryResponse>>> Handle(
            GetSchoolsQuery request,
            CancellationToken cancellationToken)
        {
            var schools = await _schoolRepository.GetDirectoryReadOnlyAsync(
                request.SportId,
                request.City,
                request.Search,
                cancellationToken);

            var response = schools
                .Select(school => new SchoolSummaryResponse(
                    school.Id,
                    school.Name,
                    school.LogoUrl,
                    school.SportId,
                    school.SportName,
                    school.City,
                    school.AccountStatus))
                .ToList();

            return Result.Success<IReadOnlyList<SchoolSummaryResponse>>(response);
        }
    }
}
