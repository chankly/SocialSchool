using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Profiles;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Mediator;

namespace HSoft.SocialSchool.Application.Queries.Profiles
{
    /// <summary>
    /// Resuelve el perfil propio a partir del <c>sub</c> del token: la cuenta se busca por su identidad
    /// externa y el tipo de cuenta de esa cuenta decide qué perfil se carga.
    /// </summary>
    public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, Result<MyProfileResponse?>>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IAthleteRepository _athleteRepository;
        private readonly ISchoolRepository _schoolRepository;

        public GetMyProfileQueryHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IAthleteRepository athleteRepository,
            ISchoolRepository schoolRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _athleteRepository = athleteRepository;
            _schoolRepository = schoolRepository;
        }

        public async ValueTask<Result<MyProfileResponse?>> Handle(
            GetMyProfileQuery request,
            CancellationToken cancellationToken)
        {
            var externalId = _currentUser.ExternalId;

            if (string.IsNullOrWhiteSpace(externalId))
                return Result.Failure<MyProfileResponse?>(
                    "The authenticated user has no external identity.");

            var user = await _userRepository.GetByExternalIdReadOnlyAsync(externalId, cancellationToken);

            if (user is null)
                return Result.Success<MyProfileResponse?>(null);

            return user.AccountType switch
            {
                AccountType.Athlete => await BuildAthleteProfileAsync(user, cancellationToken),
                AccountType.School => await BuildSchoolProfileAsync(user, cancellationToken),
                _ => Result.Failure<MyProfileResponse?>($"Unsupported account type '{user.AccountType}'.")
            };
        }

        private async Task<Result<MyProfileResponse?>> BuildAthleteProfileAsync(
            User user,
            CancellationToken cancellationToken)
        {
            var athlete = await _athleteRepository.GetByIdWithSportsReadOnlyAsync(user.Id, cancellationToken);

            if (athlete is null)
                return Result.Success<MyProfileResponse?>(null);

            var practicedSports = athlete.PracticedSports
                .Select(sport => new SportResponse(sport.Id, sport.Name))
                .ToList();

            return Result.Success<MyProfileResponse?>(new MyProfileResponse(
                user.Id,
                user.Nickname,
                user.Email,
                user.AccountType,
                user.AccountStatus,
                new AthleteProfileResponse(
                    athlete.FirstName,
                    athlete.LastName,
                    athlete.DateOfBirth,
                    athlete.Country,
                    athlete.City,
                    athlete.ProfilePictureUrl,
                    athlete.Biography,
                    practicedSports),
                null));
        }

        private async Task<Result<MyProfileResponse?>> BuildSchoolProfileAsync(
            User user,
            CancellationToken cancellationToken)
        {
            var school = await _schoolRepository.GetByIdReadOnly(user.Id, cancellationToken);

            if (school is null)
                return Result.Success<MyProfileResponse?>(null);

            return Result.Success<MyProfileResponse?>(new MyProfileResponse(
                user.Id,
                user.Nickname,
                user.Email,
                user.AccountType,
                user.AccountStatus,
                null,
                new SchoolProfileResponse(
                    school.Name,
                    school.LogoUrl,
                    school.SportId,
                    school.Description,
                    school.Country,
                    school.City,
                    school.ContactEmail,
                    school.Phone,
                    school.Website)));
        }
    }
}
