using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class SportRepository(MyCompanyDbContext dbContext) : BaseRepository<Sport>(dbContext), ISportRepository
    {
        public async Task<IReadOnlyList<Sport>> GetActiveReadOnlyAsync(
            CancellationToken cancellationToken = default) =>
            await _dbContext.Set<Sport>()
                .AsNoTracking()
                .Where(sport => sport.IsActive)
                .OrderBy(sport => sport.Name)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<Sport>> GetByIdsAsync(
            IReadOnlyList<Guid> sportIds,
            CancellationToken cancellationToken = default) =>
            // Sin AsNoTracking a propósito: al añadir el perfil del deportista, EF reutiliza estas instancias
            // en vez de intentar insertar los deportes como si fueran nuevos.
            await _dbContext.Set<Sport>()
                .Where(sport => sportIds.Contains(sport.Id))
                .ToListAsync(cancellationToken);
    }
}
