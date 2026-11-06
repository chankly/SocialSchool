using FluentValidation;
using HSoft.SocialSchool.Application.Commands.Profiles;

namespace HSoft.SocialSchool.Application.Validators.Commands
{
    /// <summary>
    /// Valida el formulario de alta del perfil antes de tocar la base de datos: la forma del nick, que venga
    /// exactamente uno de los dos bloques, la mayoría de edad del deportista y el deporte obligatorio de la
    /// escuela. Que el bloque corresponda al tipo de la cuenta o que el deporte exista lo resuelve el handler,
    /// porque exige consultar datos que él ya necesita.
    /// </summary>
    public sealed class CompleteMyProfileCommandValidator : AbstractValidator<CompleteMyProfileCommand>
    {
        private const int NicknameMaxLength = 50;
        private const int MinimumAge = 18;

        public CompleteMyProfileCommandValidator()
        {
            RuleFor(command => command.Nickname)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The nickname is required.")
                .MaximumLength(NicknameMaxLength)
                    .WithMessage($"The nickname cannot be longer than {NicknameMaxLength} characters.")
                .Must(BeAlphanumeric).WithMessage("The nickname can only contain letters and digits.");

            RuleFor(command => command)
                .Must(command => (command.Athlete is null) ^ (command.School is null))
                .WithMessage("The request must include exactly one of the athlete or school profile blocks.");

            When(command => command.Athlete is not null, () =>
            {
                RuleFor(command => command.Athlete!.FirstName)
                    .NotEmpty().WithMessage("The athlete first name is required.");

                RuleFor(command => command.Athlete!.LastName)
                    .NotEmpty().WithMessage("The athlete last name is required.");

                RuleFor(command => command.Athlete!.DateOfBirth)
                    .Must(dateOfBirth => dateOfBirth != default)
                        .WithMessage("The date of birth is required.")
                    .Must(AcknowledgeMinimumAge)
                        .WithMessage($"The athlete must be at least {MinimumAge} years old.");
            });

            When(command => command.School is not null, () =>
            {
                RuleFor(command => command.School!.Name)
                    .NotEmpty().WithMessage("The school name is required.");

                RuleFor(command => command.School!.SportId)
                    .NotEmpty().WithMessage("The school must have a sport.");
            });
        }

        private static bool BeAlphanumeric(string nickname) => nickname.All(char.IsLetterOrDigit);

        private static bool AcknowledgeMinimumAge(DateOnly dateOfBirth) =>
            dateOfBirth <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-MinimumAge);
    }
}
