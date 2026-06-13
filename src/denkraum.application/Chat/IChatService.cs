using Denkraum.Application.Common;

namespace Denkraum.Application.Chat;

public interface IChatService
{
    Task<Result<ChatSessionDto>> CreateSessionAsync(CancellationToken cancellationToken);
    Task<Result<ChatResponse>> AskAsync(ChatRequest request, CancellationToken cancellationToken);
}
