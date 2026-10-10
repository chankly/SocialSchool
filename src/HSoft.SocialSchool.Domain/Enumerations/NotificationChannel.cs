namespace HSoft.SocialSchool.Domain.Enumerations
{
    /// <summary>
    /// Canal por el que se entrega una notificación. El usuario puede desactivarlos de forma independiente.
    /// </summary>
    public enum NotificationChannel
    {
        /// <summary>Dentro de la propia aplicación. Es el único canal implementado en el alcance actual.</summary>
        InApp = 0,

        /// <summary>Por correo electrónico. Pospuesto.</summary>
        Email = 1,

        /// <summary>Notificación push al dispositivo. Pospuesto.</summary>
        Push = 2
    }
}
