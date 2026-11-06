using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Athlete"/>.</summary>
    public interface IAthleteRepository : IBaseRepository<Athlete>
    {
        /// <summary>
        /// Devuelve el perfil del deportista con los deportes que practica, o <see langword="null"/> si no
        /// existe. Los deportes van en la misma consulta porque el lazy loading está desactivado.
        /// </summary>
        Task<Athlete?> GetByIdWithSportsReadOnlyAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve los perfiles de deportista cuyo identificador está en la lista, sin rastrear. Sirve para
        /// resolver la ciudad de las cuentas invitables sin cargar sus agregados completos.
        /// </summary>
        /// <param name="athleteIds">Identificadores que se quieren resolver.</param>
        /// <param name="cancellationToken">Token de cancelación de la petición.</param>
        Task<IReadOnlyList<Athlete>> GetByIdsReadOnlyAsync(
            IReadOnlyList<Guid> athleteIds,
            CancellationToken cancellationToken = default);
    }
}
