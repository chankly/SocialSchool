namespace HSoft.SocialSchool.Domain.Enumerations
{
    /// <summary>
    /// Estado de una denuncia. Mientras está pendiente, el contenido denunciado deja de mostrarse.
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>Denunciada y a la espera de que el técnico la revise.</summary>
        Pending = 0,

        /// <summary>Revisada por el técnico.</summary>
        Resolved = 1
    }
}
