namespace HSoft.SocialSchool.Domain.Enumerations
{
    /// <summary>
    /// Estado del vínculo de asistencia entre un deportista y una escuela.
    /// </summary>
    public enum AttendanceStatus
    {
        /// <summary>Propuesta por una de las partes y pendiente de que la otra la resuelva.</summary>
        Pending = 0,

        /// <summary>Aceptada: la escuela ve el perfil del deportista y puede puntuar sus maniobras.</summary>
        Accepted = 1,

        /// <summary>Rechazada por la otra parte. El rechazo es silencioso y la propuesta puede reenviarse.</summary>
        Rejected = 2,

        /// <summary>Retirada por cualquiera de las partes. Se borran las puntuaciones que la escuela había dado.</summary>
        Withdrawn = 3
    }
}
