using HSoft.SocialSchool.Application.Models.Schools;
using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Attendances;
using HSoft.SocialSchool.Domain.Entities.Athletes;
using HSoft.SocialSchool.Domain.Entities.Users;
using HSoft.SocialSchool.Domain.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class SchoolAttendanceRepository(MyCompanyDbContext dbContext) : BaseRepository<SchoolAttendance>(dbContext), ISchoolAttendanceRepository
    {
        public async Task<IReadOnlyList<SchoolAttendeeItem>> GetAcceptedAttendeesReadOnlyAsync(
            Guid schoolId,
            CancellationToken cancellationToken = default) =>
            await (from attendance in _dbContext.Set<SchoolAttendance>().AsNoTracking()
                   join account in _dbContext.Set<User>().AsNoTracking() on attendance.AthleteId equals account.Id
                   join athlete in _dbContext.Set<Athlete>().AsNoTracking() on attendance.AthleteId equals athlete.Id
                   where attendance.SchoolId == schoolId
                       && attendance.Status == AttendanceStatus.Accepted
                   orderby account.Nickname
                   select new SchoolAttendeeItem(account.Id, account.Nickname, athlete.ProfilePictureUrl))
                .ToListAsync(cancellationToken);
    }
}
