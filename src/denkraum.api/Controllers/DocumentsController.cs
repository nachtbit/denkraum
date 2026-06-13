using Denkraum.Application.Documents;
using Microsoft.AspNetCore.Mvc;

namespace Denkraum.Api.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController(IDocumentService documentService) : ControllerBase
{
    /// <summary>Gets all indexed documents.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<DocumentDto>>> GetDocuments(CancellationToken cancellationToken)
    {
        var result = await documentService.GetDocumentsAsync(cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Gets a single indexed document.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        var result = await documentService.GetDocumentAsync(id, cancellationToken);
        return result.IsFailure ? NotFound(ToProblem(result.Error, StatusCodes.Status404NotFound)) : Ok(result.Value);
    }

    /// <summary>Indexes every supported workbook in the configured data directory.</summary>
    [HttpPost("index")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IndexDocuments(CancellationToken cancellationToken)
    {
        var result = await documentService.IndexDocumentsAsync(cancellationToken);
        return result.IsFailure ? BadRequest(ToProblem(result.Error, StatusCodes.Status400BadRequest)) : Accepted();
    }

    /// <summary>Deletes an indexed document and its chunks.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken cancellationToken)
    {
        var result = await documentService.DeleteDocumentAsync(id, cancellationToken);
        return result.IsFailure ? NotFound(ToProblem(result.Error, StatusCodes.Status404NotFound)) : NoContent();
    }

    private ProblemDetails ToProblem(string error, int status) => new()
    {
        Title = error,
        Status = status,
        Instance = HttpContext.Request.Path
    };
}
