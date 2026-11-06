using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Schools;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Enumerations;
using Mediator;

namespace HSoft.SocialSchool.Application.Queries.Schools
{
    /// <summary>
    /// Compone la ficha de una escuela con tres lecturas independientes: la escuela con sus servicios, el
    /// deporte que practica y los deportistas con asistencia aceptada. Los asistentes salen solo de
    /// <c>SchoolAttendance</c> aceptadas y se publican con nick y foto.
    /// </summary>
    public sealed class GetSchoolQueryHandler : IRequestHandler<GetSchoolQuery, Result<SchoolDetailResponse?>>
    {
        private readonly ISchoolRepository _schoolRepository;
        private readonly ISportRepository _sportRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISchoolAttendanceRepository _schoolAttendanceRepository;

        public GetSchoolQueryHandler(
            ISchoolRepository schoolRepository,
            ISportRepository sportRepository,
            IUserRepository userRepository,
            ISchoolAttendanceRepository schoolAttendanceRepository)
        {
            _schoolRepository = schoolRepository;
            _sportRepository = sportRepository;
            _userRepository = userRepository;
            _schoolAttendanceRepository = schoolAttendanceRepository;
        }

        public async ValueTask<Result<SchoolDetailResponse?>> Handle(
            GetSchoolQuery request,
            CancellationToken cancellationToken)
        {
            var school = await _schoolRepository.GetByIdWithServicesReadOnlyAsync(request.Id, cancellationToken);

            if (school is null)
                return Result.Success<SchoolDetailResponse?>(null);

            var sport = await _sportRepository.GetByIdReadOnly(school.SportId, cancellationToken);
            var account = await _userRepository.GetByIdReadOnly(school.Id, cancellationToken);
            var attendees = await _schoolAttendanceRepository.GetAcceptedAttendeesReadOnlyAsync(
                school.Id,
                cancellationToken);

            var services = school.Services
                .Select(service => new SchoolServiceResponse(
                    service.Id,
                    service.Name,
                    service.Description,
                    service.Price,
                    service.Currency))
                .ToList();

            var attendeesResponse = attendees
                .Select(attendee => new SchoolAttendeeResponse(
                    attendee.Id,
                    attendee.Nickname,
                    attendee.ProfilePictureUrl))
                .ToList();

            return Result.Success<SchoolDetailResponse?>(new SchoolDetailResponse(
                school.Id,
                school.Name,
                school.LogoUrl,
                school.SportId,
                sport?.Name ?? string.Empty,
                school.Description,
                school.Country,
                school.City,
                school.ContactEmail,
                school.Phone,
                school.Website,
                account?.AccountStatus ?? AccountStatus.Active,
                services,
                attendeesResponse));
        }
    }
}
