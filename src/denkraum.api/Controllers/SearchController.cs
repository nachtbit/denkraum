using Denkraum.Application.Search;
using Microsoft.AspNetCore.Mvc;

namespace Denkraum.Api.Controllers;

[ApiController]
[Route("api/search")]
public sealed class SearchController(ISearchService searchService) : ControllerBase
{
    /// <summary>Runs semantic search over indexed workbook chunks.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchResponse>> Search(SearchRequest request, CancellationToken cancellationToken)
    {
        var result = await searchService.SearchAsync(request, cancellationToken);
        return result.IsFailure ? BadRequest(ToProblem(result.Error)) : Ok(result.Value);
    }

    private ProblemDetails ToProblem(string error) => new()
    {
        Title = error,
        Status = StatusCodes.Status400BadRequest,
        Instance = HttpContext.Request.Path
    };
}
