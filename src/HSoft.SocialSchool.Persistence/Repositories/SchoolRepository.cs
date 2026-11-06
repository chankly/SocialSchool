using HSoft.SocialSchool.Application.Models.Schools;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Schools;
using HSoft.SocialSchool.Domain.Entities.Sports;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class SchoolRepository(MyCompanyDbContext dbContext) : BaseRepository<School>(dbContext), ISchoolRepository
    {
        private const int DirectorySize = 50;

        public async Task<IReadOnlyList<SchoolDirectoryItem>> GetDirectoryReadOnlyAsync(
            Guid? sportId,
            string? city,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var query =
                from school in _dbContext.Set<School>().AsNoTracking()
                join sport in _dbContext.Set<Sport>().AsNoTracking() on school.SportId equals sport.Id
                join account in _dbContext.Set<User>().AsNoTracking() on school.Id equals account.Id
                where account.AccountType == AccountType.School
                    && (account.AccountStatus == AccountStatus.Active
                        || account.AccountStatus == AccountStatus.PendingValidation)
                select new { School = school, Sport = sport, Account = account };

            if (sportId.HasValue)
                query = query.Where(row => row.School.SportId == sportId.Value);

            // La colación de MySQL no distingue mayúsculas, así que la igualdad y el LIKE son insensibles.
            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(row => row.School.City == city);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(row => row.School.Name.Contains(search));

            return await query
                .OrderBy(row => row.School.Name)
                .Take(DirectorySize)
                .Select(row => new SchoolDirectoryItem(
                    row.School.Id,
                    row.School.Name,
                    row.School.LogoUrl,
                    row.School.SportId,
                    row.Sport.Name,
                    row.School.City,
                    row.Account.AccountStatus))
                .ToListAsync(cancellationToken);
        }

        public Task<School?> GetByIdWithServicesReadOnlyAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            _dbContext.Set<School>()
                .AsNoTracking()
                .Include(school => school.Services)
                .FirstOrDefaultAsync(school => school.Id == id, cancellationToken);

        public async Task<IReadOnlyList<School>> GetByIdsReadOnlyAsync(
            IReadOnlyList<Guid> schoolIds,
            CancellationToken cancellationToken = default) =>
            await _dbContext.Set<School>()
                .AsNoTracking()
                .Where(school => schoolIds.Contains(school.Id))
                .ToListAsync(cancellationToken);
    }
}

