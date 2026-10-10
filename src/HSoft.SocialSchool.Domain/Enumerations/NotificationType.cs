namespace HSoft.SocialSchool.Domain.Enumerations
{
    /// <summary>
    /// Evento que origina una notificación. Los rechazos de amistad y de asistencia son silenciosos y no generan ninguna.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>Se ha recibido una solicitud de amistad.</summary>
        FriendRequestReceived = 0,

        /// <summary>Se ha aceptado una solicitud de amistad.</summary>
        FriendRequestAccepted = 1,

        /// <summary>Se ha solicitado un vínculo de asistencia.</summary>
        AttendanceRequested = 2,

        /// <summary>Se ha aceptado un vínculo de asistencia.</summary>
        AttendanceAccepted = 3,

        /// <summary>Se ha rechazado un vínculo de asistencia.</summary>
        AttendanceRejected = 4,

        /// <summary>Hay un comentario nuevo.</summary>
        NewComment = 5,

        /// <summary>Hay un "me gusta" nuevo.</summary>
        NewLike = 6,

        /// <summary>Se ha recibido una puntuación nueva.</summary>
        NewRating = 7,

        /// <summary>Se ha recibido una denuncia dirigida al técnico.</summary>
        ReportReceived = 8
    }
}
