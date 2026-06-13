using Denkraum.Application.Common;

namespace Denkraum.Application.Dashboard;

public interface IDashboardService
{
    Task<Result<DashboardSummary>> GetSummaryAsync(CancellationToken cancellationToken);
}
