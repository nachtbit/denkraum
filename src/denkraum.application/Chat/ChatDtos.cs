using Denkraum.Application.Common;

namespace Denkraum.Application.Chat;

public sealed record ChatRequest(string Question, Guid? SessionId = null, int TopK = 5);

public sealed record ChatResponse(Guid SessionId, string Answer, IReadOnlyCollection<SourceReference> Sources);

public sealed record ChatSessionDto(Guid Id, DateTimeOffset CreatedAt);
