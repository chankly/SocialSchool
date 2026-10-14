using HSoft.SocialSchool.Domain.Entities.Reports;
using HSoft.SocialSchool.Domain.Primitives;

namespace HSoft.SocialSchool.Application.Repositories
{
    /// <summary>Contrato de persistencia del agregado <see cref="Report"/>.</summary>
    public interface IReportRepository : IBaseRepository<Report>
    {
    }
}
