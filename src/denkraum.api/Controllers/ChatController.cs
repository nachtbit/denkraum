using Denkraum.Application.Chat;
using Microsoft.AspNetCore.Mvc;

namespace Denkraum.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController(IChatService chatService) : ControllerBase
{
    /// <summary>Creates a new chat session.</summary>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(ChatSessionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ChatSessionDto>> CreateSession(CancellationToken cancellationToken)
    {
        var result = await chatService.CreateSessionAsync(cancellationToken);
        return Created($"/api/chat/sessions/{result.Value.Id}", result.Value);
    }

    /// <summary>Answers a question using retrieved workbook context and source citations.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Ask(ChatRequest request, CancellationToken cancellationToken)
    {
        var result = await chatService.AskAsync(request, cancellationToken);
        return result.IsFailure ? BadRequest(ToProblem(result.Error)) : Ok(result.Value);
    }

    private ProblemDetails ToProblem(string error) => new()
    {
        Title = error,
        Status = StatusCodes.Status400BadRequest,
        Instance = HttpContext.Request.Path
    };
}
