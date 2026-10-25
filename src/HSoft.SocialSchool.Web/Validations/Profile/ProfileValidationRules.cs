using FluentValidation;
using HSoft.SocialSchool.Web.Models.Common;

namespace HSoft.SocialSchool.Web.Validations.Profile;

/// <summary>
/// Reglas compartidas por los formularios de perfil. Se mantienen aquí para que el nick,
/// el país y las URL se validen igual en deportistas y escuelas.
/// </summary>
public static class ProfileValidationRules
{
    /// <summary>Nick único, inmutable y estrictamente alfanumérico.</summary>
    public static IRuleBuilderOptions<T, string> ValidNickname<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nick es obligatorio.")
            .Length(3, 20).WithMessage("El nick debe tener entre 3 y 20 caracteres.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("El nick solo admite letras y dígitos, sin espacios ni símbolos.");
    }

    /// <summary>País obligatorio y presente en el catálogo del cliente.</summary>
    public static IRuleBuilderOptions<T, string> ValidCountry<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Selecciona un país.")
            .Must(CountryCatalog.Contains).WithMessage("Selecciona un país del listado.");
    }

    /// <summary>URL opcional, pero con esquema http o https cuando se informa.</summary>
    public static IRuleBuilderOptions<T, string?> OptionalHttpUrl<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(BeNullOrHttpUrl).WithMessage("Debe ser una URL http(s) válida.")
            .MaximumLength(500).WithMessage("La URL no puede superar los 500 caracteres.");
    }

    private static bool BeNullOrHttpUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
