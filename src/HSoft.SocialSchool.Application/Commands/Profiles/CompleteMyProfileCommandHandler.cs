using CSharpFunctionalExtensions;
using HSoft.SocialSchool.Application.Models.Profiles;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Application.Services;
using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Entities.Schools;
using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Mediator;

namespace HSoft.SocialSchool.Application.Commands.Profiles
{
    /// <summary>
    /// Crea el perfil del usuario autenticado con el identificador de su cuenta, fija su nick y devuelve el
    /// mismo perfil que la consulta para que el cliente no tenga que volver a preguntar. El tipo de cuenta
    /// manda: si el bloque recibido no es el suyo, no se crea nada.
    /// </summary>
    public sealed class CompleteMyProfileCommandHandler
        : IRequestHandler<CompleteMyProfileCommand, Result<MyProfileResponse>>
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;
        private readonly IAthleteRepository _athleteRepository;
        private readonly ISchoolRepository _schoolRepository;
        private readonly ISportRepository _sportRepository;

        public CompleteMyProfileCommandHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository,
            IAthleteRepository athleteRepository,
            ISchoolRepository schoolRepository,
            ISportRepository sportRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
            _athleteRepository = athleteRepository;
            _schoolRepository = schoolRepository;
            _sportRepository = sportRepository;
        }

        public async ValueTask<Result<MyProfileResponse>> Handle(
            CompleteMyProfileCommand request,
            CancellationToken cancellationToken)
        {
            var externalId = _currentUser.ExternalId;

            if (string.IsNullOrWhiteSpace(externalId))
                return Result.Failure<MyProfileResponse>(
                    "The authenticated user has no external identity.");

            var user = await _userRepository.GetByExternalIdReadOnlyAsync(externalId, cancellationToken);

            if (user is null)
                return Result.Failure<MyProfileResponse>(
                    "The authenticated user has no business account.");

            return user.AccountType switch
            {
                AccountType.Athlete => await CompleteAthleteProfileAsync(user, request, cancellationToken),
                AccountType.School => await CompleteSchoolProfileAsync(user, request, cancellationToken),
                _ => Result.Failure<MyProfileResponse>($"Unsupported account type '{user.AccountType}'.")
            };
        }

        /// <summary>
        /// Fija el nick una sola vez. Si el perfil ya se estaba creando y el nick quedó fijado, el reintento
        /// con el mismo valor continúa; con otro valor se rechaza, porque el nick es inmutable.
        /// </summary>
        private async Task<Result> ReserveNicknameAsync(
            User user,
            string nickname,
            CancellationToken cancellationToken)
        {
            if (user.Nickname is not null)
            {
                return string.Equals(user.Nickname, nickname, StringComparison.OrdinalIgnoreCase)
                    ? Result.Success()
                    : Result.Failure($"The account already has the nickname '{user.Nickname}'.");
            }

            if (await _userRepository.ExistsByNicknameAsync(nickname, cancellationToken))
                return Result.Failure($"The nickname '{nickname}' is already in use.");

            user.SetNickname(nickname);
            await _userRepository.Update(user);

            return Result.Success();
        }

        private async Task<Result<MyProfileResponse>> CompleteAthleteProfileAsync(
            User user,
            CompleteMyProfileCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Athlete is null)
                return Result.Failure<MyProfileResponse>(
                    "The account is an athlete and requires the athlete profile block.");

            var sportIds = (request.Athlete.PracticedSportIds ?? []).Distinct().ToList();
            var sports = await _sportRepository.GetByIdsAsync(sportIds, cancellationToken);
            var unknownSportIds = sportIds.Where(id => sports.All(sport => sport.Id != id)).ToList();

            // Un identificador desconocido no se ignora en silencio: el perfil quedaría incompleto.
            if (unknownSportIds.Count > 0)
                return Result.Failure<MyProfileResponse>(
                    $"The following sports do not exist: {string.Join(", ", unknownSportIds)}.");

            var nicknameResult = await ReserveNicknameAsync(user, request.Nickname, cancellationToken);

            if (nicknameResult.IsFailure)
                return Result.Failure<MyProfileResponse>(nicknameResult.Error);

            var athlete = Athlete.Create(
                user.Id,
                request.Athlete.FirstName,
                request.Athlete.LastName,
                request.Athlete.DateOfBirth,
                request.Athlete.Country,
                request.Athlete.City);

            athlete.UpdateProfile(
                request.Athlete.FirstName,
                request.Athlete.LastName,
                request.Athlete.Country,
                request.Athlete.City,
                request.Athlete.Biography,
                request.Athlete.ProfilePictureUrl);

            // Se respeta el orden del formulario; los identificadores ya se han deduplicado.
            foreach (var sport in sportIds.Select(id => sports.Single(sport => sport.Id == id)))
                athlete.AddPracticedSport(sport);

            await _athleteRepository.Add(athlete);

            return Result.Success(BuildAthleteProfile(user, athlete));
        }

        private async Task<Result<MyProfileResponse>> CompleteSchoolProfileAsync(
            User user,
            CompleteMyProfileCommand request,
            CancellationToken cancellationToken)
        {
            if (request.School is null)
                return Result.Failure<MyProfileResponse>(
                    "The account is a school and requires the school profile block.");

            var sport = await _sportRepository.GetByIdReadOnly(request.School.SportId, cancellationToken);

            if (sport is null)
                return Result.Failure<MyProfileResponse>(
                    $"The sport '{request.School.SportId}' does not exist.");

            var nicknameResult = await ReserveNicknameAsync(user, request.Nickname, cancellationToken);

            if (nicknameResult.IsFailure)
                return Result.Failure<MyProfileResponse>(nicknameResult.Error);

            var school = School.Create(
                user.Id,
                request.School.Name,
                request.School.SportId,
                request.School.Country,
                request.School.City);

            school.UpdateProfile(
                request.School.Name,
                request.School.Country,
                request.School.City,
                request.School.Description,
                request.School.ContactEmail,
                request.School.Phone,
                request.School.Website,
                request.School.LogoUrl);

            await _schoolRepository.Add(school);

            return Result.Success(BuildSchoolProfile(user, school));
        }

        private static MyProfileResponse BuildAthleteProfile(User user, Athlete athlete) =>
            new(
                user.Id,
                user.Nickname ?? string.Empty,
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
                    athlete.PracticedSports
                        .Select(sport => new SportResponse(sport.Id, sport.Name))
                        .ToList()),
                null);

        private static MyProfileResponse BuildSchoolProfile(User user, School school) =>
            new(
                user.Id,
                user.Nickname ?? string.Empty,
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
                    school.Website));
    }
}
