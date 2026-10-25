namespace HSoft.SocialSchool.Web.Models.Accounts;

/// <summary>
/// Estado de la cuenta de negocio que devuelve la API en el perfil propio.
/// </summary>
public enum AccountStatus
{
    /// <summary>Cuenta activa.</summary>
    Active = 1,

    /// <summary>Escuela creada y todavía pendiente de validación.</summary>
    PendingValidation = 2,

    /// <summary>Cuenta eliminada de forma lógica.</summary>
    Deleted = 3
}
