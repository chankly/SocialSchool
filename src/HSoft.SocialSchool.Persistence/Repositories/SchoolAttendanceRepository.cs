using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Attendances;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class SchoolAttendanceRepository(MyCompanyDbContext dbContext) : BaseRepository<SchoolAttendance>(dbContext), ISchoolAttendanceRepository
    {
    }
}
