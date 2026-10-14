using HSoft.SocialSchool.Domain.Entities.Attendances;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="SchoolAttendance"/>.</summary>
    public interface ISchoolAttendanceRepository : IBaseRepository<SchoolAttendance>
    {
    }
}
