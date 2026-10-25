using FluentValidation;
using HSoft.SocialSchool.Web.Services.Profile.Requests;

namespace HSoft.SocialSchool.Web.Validations.Profile;

/// <summary>Validación del formulario de alta de un deportista.</summary>
public sealed class CreateAthleteProfileRequestValidator : AbstractValidator<CreateAthleteProfileRequest>
{
    /// <summary>Configura las reglas del formulario.</summary>
    public CreateAthleteProfileRequestValidator()
    {
        RuleFor(request => request.Nickname).ValidNickname();

        RuleFor(request => request.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(request => request.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Los apellidos son obligatorios.")
            .MaximumLength(150).WithMessage("Los apellidos no pueden superar los 150 caracteres.");

        RuleFor(request => request.DateOfBirth)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("La fecha de nacimiento es obligatoria.")
            .Must(BeAPlausibleBirthDate).WithMessage("La fecha debe ser anterior a hoy y tener menos de 120 años.");

        RuleFor(request => request.Country).ValidCountry();

        RuleFor(request => request.City)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La ciudad es obligatoria.")
            .MaximumLength(100).WithMessage("La ciudad no puede superar los 100 caracteres.");

        RuleFor(request => request.ProfilePictureUrl).OptionalHttpUrl();

        RuleFor(request => request.Biography)
            .MaximumLength(500).WithMessage("La biografía no puede superar los 500 caracteres.");

        RuleFor(request => request.PracticedSportIds)
            .NotEmpty().WithMessage("Selecciona al menos un deporte.");
    }

    private static bool BeAPlausibleBirthDate(DateTime? dateOfBirth) =>
        dateOfBirth is not null
        && dateOfBirth.Value.Date < DateTime.Today
        && dateOfBirth.Value.Date >= DateTime.Today.AddYears(-120);
}
