using Denkraum.Application.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Denkraum.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    /// <summary>Gets operational counts for the indexed corpus and chats.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardSummary), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummary>> GetSummary(CancellationToken cancellationToken)
    {
        var result = await dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(result.Value);
    }
}
