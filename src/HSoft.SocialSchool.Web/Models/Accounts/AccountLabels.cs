namespace HSoft.SocialSchool.Web.Models.Accounts;

/// <summary>
/// Etiquetas en español para los valores de cuenta que se muestran en la interfaz.
/// </summary>
public static class AccountLabels
{
    /// <summary>Nombre legible de un tipo de cuenta.</summary>
    public static string ToDisplayName(this AccountType accountType) => accountType switch
    {
        AccountType.Athlete => "Deportista",
        AccountType.School => "Escuela",
        _ => "Cuenta"
    };

    /// <summary>Nombre legible del estado de una cuenta.</summary>
    public static string ToDisplayName(this AccountStatus accountStatus) => accountStatus switch
    {
        AccountStatus.Active => "Cuenta activa",
        AccountStatus.PendingValidation => "Pendiente de validación",
        AccountStatus.Deleted => "Cuenta eliminada",
        _ => "Estado desconocido"
    };
}
