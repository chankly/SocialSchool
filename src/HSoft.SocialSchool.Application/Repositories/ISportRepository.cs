using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Sport"/>.</summary>
    public interface ISportRepository : IBaseRepository<Sport>
    {
        /// <summary>
        /// Devuelve los deportes activos del catálogo, ordenados por nombre. Es lo que ofrece el selector del
        /// formulario de perfil; los inactivos no se publican.
        /// </summary>
        Task<IReadOnlyList<Sport>> GetActiveReadOnlyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve los deportes cuyo identificador está en la lista, <b>rastreados</b> por el contexto: al
        /// añadir después el perfil del deportista, EF no debe intentar insertarlos como si fueran nuevos.
        /// Los identificadores desconocidos simplemente no aparecen en el resultado.
        /// </summary>
        Task<IReadOnlyList<Sport>> GetByIdsAsync(
            IReadOnlyList<Guid> sportIds,
            CancellationToken cancellationToken = default);
    }
}
