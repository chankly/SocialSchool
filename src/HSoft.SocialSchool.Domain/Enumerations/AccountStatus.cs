namespace HSoft.SocialSchool.Domain.Enumerations
{
    /// <summary>
    /// Estado de una cuenta. La eliminación es suave: la cuenta deja de ser visible pero conserva todo su contenido.
    /// </summary>
    public enum AccountStatus
    {
        /// <summary>La cuenta está operativa.</summary>
        Active = 0,

        /// <summary>Cuenta de escuela registrada y todavía no validada por el técnico.</summary>
        PendingValidation = 1,

        /// <summary>Cuenta eliminada por su titular. Solo el técnico puede restaurarla.</summary>
        Deleted = 2
    }
}
