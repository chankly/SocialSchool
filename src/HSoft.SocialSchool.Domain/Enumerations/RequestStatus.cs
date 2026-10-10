namespace HSoft.SocialSchool.Domain.Enumerations
{
    /// <summary>
    /// Estado de una solicitud de amistad.
    /// </summary>
    public enum RequestStatus
    {
        /// <summary>La solicitud está a la espera de que el receptor la resuelva.</summary>
        Pending = 0,

        /// <summary>El receptor la aceptó y se creó la amistad.</summary>
        Accepted = 1,

        /// <summary>El receptor la rechazó. El rechazo es silencioso y el solicitante puede volver a intentarlo.</summary>
        Rejected = 2,

        /// <summary>El propio solicitante la retiró antes de ser resuelta.</summary>
        Cancelled = 3
    }
}
