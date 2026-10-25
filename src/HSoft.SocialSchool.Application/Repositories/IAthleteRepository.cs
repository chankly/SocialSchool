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
    }
}
