using HSoft.SocialSchool.Application.Repositories;
using HSoft.SocialSchool.Domain.Entities.Reports;

namespace HSoft.SocialSchool.Persistence.Repositories
{
    public sealed class ReportRepository(MyCompanyDbContext dbContext) : BaseRepository<Report>(dbContext), IReportRepository
    {
    }
}
