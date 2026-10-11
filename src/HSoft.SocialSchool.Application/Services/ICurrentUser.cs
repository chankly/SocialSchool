namespace HSoft.SocialSchool.Application.Services
{
    /// <summary>
    /// Identidad del usuario autenticado en la petición actual, tal y como la expone el proveedor de
    /// identidad. La capa de aplicación la consume sin depender del contexto HTTP.
    /// </summary>
    public interface ICurrentUser
    {
        /// <summary>
        /// Identificador externo y estable del usuario: el claim <c>sub</c> del token.
        /// Es <see langword="null"/> cuando no hay usuario autenticado.
        /// </summary>
        string? ExternalId { get; }

        /// <summary>Indica si la petición tiene un usuario autenticado.</summary>
        bool IsAuthenticated { get; }
    }
}
