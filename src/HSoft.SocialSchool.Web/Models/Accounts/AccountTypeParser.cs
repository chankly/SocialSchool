namespace HSoft.SocialSchool.Web.Models.Accounts;

/// <summary>
/// Normaliza el tipo de cuenta que llega en el token de Keycloak.
/// El formulario de registro lo envía como <c>Athlete</c>/<c>School</c> o como
/// <c>Deportista</c>/<c>Escuela</c>, según el idioma con el que se rellenó.
/// </summary>
public static class AccountTypeParser
{
    /// <summary>
    /// Intenta convertir el valor bruto del token en un <see cref="AccountType"/>.
    /// </summary>
    /// <param name="rawValue">Valor del atributo <c>userType</c>.</param>
    /// <param name="accountType">Tipo normalizado cuando el valor es reconocido.</param>
    /// <returns><see langword="true"/> si el valor corresponde a un tipo de cuenta conocido.</returns>
    public static bool TryParse(string? rawValue, out AccountType accountType)
    {
        accountType = default;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        switch (rawValue.Trim().ToUpperInvariant())
        {
            case "ATHLETE":
            case "DEPORTISTA":
                accountType = AccountType.Athlete;
                return true;

            case "SCHOOL":
            case "ESCUELA":
                accountType = AccountType.School;
                return true;

            default:
                return false;
        }
    }
}
