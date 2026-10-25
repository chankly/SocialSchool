using FluentValidation.Results;
using Microsoft.AspNetCore.Components.Forms;

namespace HSoft.SocialSchool.Web.Validations.Profile;

/// <summary>
/// Traduce los fallos de FluentValidation a mensajes que <c>EditForm</c> puede mostrar
/// junto a cada campo.
/// </summary>
public static class FormValidationExtensions
{
    /// <summary>Añade al almacén los errores del resultado de validación.</summary>
    public static void AddErrors(
        this ValidationMessageStore messageStore,
        object model,
        IEnumerable<ValidationFailure> failures)
    {
        foreach (var failure in failures)
        {
            messageStore.Add(new FieldIdentifier(model, failure.PropertyName), failure.ErrorMessage);
        }
    }
}
