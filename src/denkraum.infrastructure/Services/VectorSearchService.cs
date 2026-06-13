using Denkraum.Application.Abstractions;
using Denkraum.Application.Common;
using Denkraum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pgvector;

namespace Denkraum.Infrastructure.Services;

public sealed class VectorSearchService(
    DenkraumDbContext dbContext,
    IEmbeddingService embeddingService,
    ILogger<VectorSearchService> logger) : IVectorSearchService
{
    public async Task<IReadOnlyCollection<SearchResult>> SearchAsync(string query, int topK, CancellationToken cancellationToken)
    {
        logger.LogInformation("Running vector search for top {TopK} chunks", topK);
        var embedding = await embeddingService.GenerateEmbeddingAsync(query, cancellationToken);
        var connection = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT c."Id", d."FileName", c."SheetName", c."RowNumber", c."Content", 1 - (e."Vector" <=> @query_vector) AS "Score"
            FROM "Embeddings" e
            INNER JOIN "DocumentChunks" c ON c."Id" = e."ChunkId"
            INNER JOIN "Documents" d ON d."Id" = c."DocumentId"
            ORDER BY e."Vector" <=> @query_vector
            LIMIT @top_k;
            """;
        command.Parameters.AddWithValue("query_vector", new Vector(embedding));
        command.Parameters.AddWithValue("top_k", topK);

        var results = new List<SearchResult>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new SearchResult(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetString(4),
                reader.GetDouble(5)));
        }

        return results;
    }
}
