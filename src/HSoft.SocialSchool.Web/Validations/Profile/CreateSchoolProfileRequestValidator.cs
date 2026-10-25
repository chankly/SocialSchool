using FluentValidation;
using HSoft.SocialSchool.Web.Services.Profile.Requests;

namespace HSoft.SocialSchool.Web.Validations.Profile;

/// <summary>Validación del formulario de alta de una escuela.</summary>
public sealed class CreateSchoolProfileRequestValidator : AbstractValidator<CreateSchoolProfileRequest>
{
    /// <summary>Configura las reglas del formulario.</summary>
    public CreateSchoolProfileRequestValidator()
    {
        RuleFor(request => request.Nickname).ValidNickname();

        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre de la escuela es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(request => request.LogoUrl).OptionalHttpUrl();

        RuleFor(request => request.SportId)
            .Must(sportId => sportId != Guid.Empty)
            .WithMessage("Selecciona el deporte principal de la escuela.");

        RuleFor(request => request.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres.");

        RuleFor(request => request.Country).ValidCountry();

        RuleFor(request => request.City)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La ciudad es obligatoria.")
            .MaximumLength(100).WithMessage("La ciudad no puede superar los 100 caracteres.");

        RuleFor(request => request.ContactEmail)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El correo de contacto es obligatorio.")
            .EmailAddress().WithMessage("Introduce un correo electrónico válido.")
            .MaximumLength(254).WithMessage("El correo no puede superar los 254 caracteres.");

        RuleFor(request => request.Phone)
            .MaximumLength(30).WithMessage("El teléfono no puede superar los 30 caracteres.")
            .Matches(@"^[0-9+() .-]{6,30}$").WithMessage("Introduce un teléfono válido.")
            .When(request => !string.IsNullOrWhiteSpace(request.Phone));

        RuleFor(request => request.Website).OptionalHttpUrl();
    }
}
