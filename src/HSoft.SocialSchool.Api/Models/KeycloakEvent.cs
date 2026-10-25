namespace HSoft.SocialSchool.Api.Models
{
    /// <summary>
    /// Evento que Keycloak envía al webhook. Solo se modela lo que se necesita para el alta: el tipo y el
    /// usuario; el resto del cuerpo se ignora.
    /// </summary>
    /// <param name="Type">Tipo del evento, por ejemplo <c>access.REGISTER</c>.</param>
    /// <param name="AuthDetails">Datos de autenticación del evento.</param>
    public sealed record KeycloakEvent(string? Type, KeycloakEventDetails? AuthDetails)
    {
        /// <summary>Tipo del evento que anuncia un registro nuevo.</summary>
        public const string RegistrationType = "access.REGISTER";
    }
}
