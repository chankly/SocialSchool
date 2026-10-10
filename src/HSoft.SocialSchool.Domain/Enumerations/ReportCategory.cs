namespace HSoft.SocialSchool.Domain.Enumerations
{
    /// <summary>
    /// Motivo por el que se denuncia un contenido.
    /// </summary>
    public enum ReportCategory
    {
        /// <summary>Contenido obsceno.</summary>
        Obscene = 0,

        /// <summary>Contenido racista.</summary>
        Racist = 1,

        /// <summary>Contenido discriminatorio.</summary>
        Discriminatory = 2,

        /// <summary>Cualquier otro motivo, descrito en el texto libre de la denuncia.</summary>
        Other = 3
    }
}
