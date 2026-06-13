using System.Text;
using Denkraum.Application.Abstractions;
using Denkraum.Application.Common;
using Denkraum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Denkraum.Application.Chat;

public sealed class ChatService(
    DbContext dbContext,
    IVectorSearchService vectorSearchService,
    IChatCompletionService chatCompletionService) : IChatService
{
    public async Task<Result<ChatSessionDto>> CreateSessionAsync(CancellationToken cancellationToken)
    {
        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<ChatSessionDto>.Success(new ChatSessionDto(session.Id, session.CreatedAt));
    }

    public async Task<Result<ChatResponse>> AskAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        var session = request.SessionId.HasValue
            ? await dbContext.Set<ChatSession>().FirstOrDefaultAsync(item => item.Id == request.SessionId.Value, cancellationToken)
            : null;

        if (request.SessionId.HasValue && session is null)
        {
            return Result<ChatResponse>.Failure("Chat session was not found.");
        }

        session ??= new ChatSession { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow };
        if (dbContext.Entry(session).State == EntityState.Detached)
        {
            dbContext.Add(session);
        }

        var searchResults = await vectorSearchService.SearchAsync(request.Question, request.TopK, cancellationToken);
        var prompt = BuildPrompt(request.Question, searchResults);
        var answer = await chatCompletionService.CompleteAsync(prompt, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        dbContext.Add(new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            Role = "user",
            Content = request.Question,
            CreatedAt = now
        });

        dbContext.Add(new ChatMessage
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            Role = "assistant",
            Content = answer,
            CreatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var sources = searchResults
            .Select(result => new SourceReference(result.Document, result.Sheet, result.Row))
            .Distinct()
            .ToList();

        return Result<ChatResponse>.Success(new ChatResponse(session.Id, answer, sources));
    }

    private static string BuildPrompt(string question, IReadOnlyCollection<SearchResult> searchResults)
    {
        var builder = new StringBuilder();
        builder.AppendLine("You are Denkraum, a retrieval-augmented assistant.");
        builder.AppendLine("Answer only from the provided context.");
        builder.AppendLine("Do not hallucinate or use outside knowledge.");
        builder.AppendLine("If the answer is not present in the context, say the information is unavailable.");
        builder.AppendLine();
        builder.AppendLine("Context:");

        foreach (var result in searchResults)
        {
            builder.AppendLine($"Source: {result.Document}, sheet {result.Sheet}, row {result.Row}");
            builder.AppendLine(result.Content);
            builder.AppendLine();
        }

        builder.AppendLine($"Question: {question}");
        builder.AppendLine("Answer:");
        return builder.ToString();
    }
}
