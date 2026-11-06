using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Athletes;
using Microsoft.EntityFrameworkCore;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class AthleteRepository(MyCompanyDbContext dbContext) : BaseRepository<Athlete>(dbContext), IAthleteRepository
    {
        public Task<Athlete?> GetByIdWithSportsReadOnlyAsync(Guid id, CancellationToken cancellationToken = default) =>
            _dbContext.Set<Athlete>()
                .AsNoTracking()
                .Include(athlete => athlete.PracticedSports)
                .FirstOrDefaultAsync(athlete => athlete.Id == id, cancellationToken);

        public async Task<IReadOnlyList<Athlete>> GetByIdsReadOnlyAsync(
            IReadOnlyList<Guid> athleteIds,
            CancellationToken cancellationToken = default) =>
            await _dbContext.Set<Athlete>()
                .AsNoTracking()
                .Where(athlete => athleteIds.Contains(athlete.Id))
                .ToListAsync(cancellationToken);
    }
}
