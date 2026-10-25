namespace HSoft.SocialSchool.Web.Models.Accounts;

/// <summary>
/// Tipo de cuenta del producto. Se normaliza en el cliente porque el formulario de
/// registro de Keycloak puede enviar el valor en inglés o en español.
/// </summary>
public enum AccountType
{
    /// <summary>Deportista.</summary>
    Athlete = 1,

    /// <summary>Escuela de deporte.</summary>
    School = 2
}
