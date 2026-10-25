namespace HSoft.SocialSchool.Api.Models
{
    /// <summary>
    /// Evento que Keycloak envía al webhook. Solo se modela lo que se necesita para el alta y para la
    /// sincronización: el tipo, el usuario y, en las actualizaciones, los campos que cambiaron.
    /// </summary>
    /// <param name="Type">Tipo del evento, por ejemplo <c>access.REGISTER</c>.</param>
    /// <param name="AuthDetails">Datos de autenticación del evento.</param>
    /// <param name="Details">Cambios del evento, cuando es una actualización de perfil.</param>
    public sealed record KeycloakEvent(
        string? Type,
        KeycloakEventDetails? AuthDetails,
        KeycloakEventChange? Details)
    {
        /// <summary>Tipo del evento que anuncia un registro nuevo.</summary>
        public const string RegistrationType = "access.REGISTER";

        /// <summary>Tipo del evento que anuncia un cambio en los datos de la cuenta.</summary>
        public const string ProfileUpdateType = "access.UPDATE_PROFILE";
    }
}
