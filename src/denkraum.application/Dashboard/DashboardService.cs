using Denkraum.Application.Common;
using Denkraum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Denkraum.Application.Dashboard;

public sealed class DashboardService(DbContext dbContext) : IDashboardService
{
    public async Task<Result<DashboardSummary>> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var documents = await dbContext.Set<Document>().CountAsync(cancellationToken);
        var chunks = await dbContext.Set<DocumentChunk>().CountAsync(cancellationToken);
        var embeddings = await dbContext.Set<Embedding>().CountAsync(cancellationToken);
        var chatSessions = await dbContext.Set<ChatSession>().CountAsync(cancellationToken);

        return Result<DashboardSummary>.Success(new DashboardSummary(documents, chunks, embeddings, chatSessions));
    }
}
