using Denkraum.Application.Abstractions;
using Denkraum.Application.Common;
using Denkraum.Application.Configuration;
using Denkraum.Domain.Entities;
using Denkraum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Denkraum.Infrastructure.Services;

public sealed class DocumentIndexer(
    DenkraumDbContext dbContext,
    IExcelDocumentReader excelDocumentReader,
    IEmbeddingService embeddingService,
    IOptions<IndexingOptions> options,
    ILogger<DocumentIndexer> logger) : IDocumentIndexer
{
    public async Task<Result> IndexDirectoryAsync(string directory, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(directory);
        var files = Directory.EnumerateFiles(directory, "*.xlsx", SearchOption.TopDirectoryOnly)
            .Where(file => !Path.GetFileName(file).StartsWith("~$", StringComparison.Ordinal))
            .ToList();

        foreach (var file in files)
        {
            var result = await IndexFileAsync(file, cancellationToken);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return Result.Success();
    }

    public async Task<Result> IndexFileAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath) || !filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("Excel file was not found or is not supported.");
        }

        var fullPath = Path.GetFullPath(filePath);
        logger.LogInformation("Indexing Excel workbook {FilePath}", fullPath);

        var existing = await dbContext.Documents.FirstOrDefaultAsync(document => document.FilePath == fullPath, cancellationToken);
        if (existing is not null)
        {
            dbContext.Documents.Remove(existing);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var chunks = await excelDocumentReader.ReadAsync(fullPath, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = Path.GetFileName(fullPath),
            FilePath = fullPath,
            IndexedAt = now,
            CreatedAt = now
        };

        dbContext.Documents.Add(document);

        foreach (var batch in chunks.Chunk(Math.Max(1, options.Value.BatchSize)))
        {
            var batchItems = batch.ToList();
            var embeddings = await embeddingService.GenerateEmbeddingsAsync(batchItems.Select(item => item.Content).ToList(), cancellationToken);

            foreach (var pair in batchItems.Zip(embeddings))
            {
                var chunk = new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    SheetName = pair.First.SheetName,
                    RowNumber = pair.First.RowNumber,
                    Content = pair.First.Content,
                    CreatedAt = now,
                    Embedding = new Embedding { Vector = pair.Second }
                };
                document.Chunks.Add(chunk);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Indexed {ChunkCount} chunks for {FileName}", chunks.Count, document.FileName);
        return Result.Success();
    }

    public async Task<Result> DeleteByPathAsync(string filePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(filePath);
        var document = await dbContext.Documents.FirstOrDefaultAsync(item => item.FilePath == fullPath, cancellationToken);
        if (document is null)
        {
            return Result.Success();
        }

        dbContext.Documents.Remove(document);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Removed indexed workbook {FilePath}", fullPath);
        return Result.Success();
    }
}
