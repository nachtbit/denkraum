using Denkraum.Application.Abstractions;
using Denkraum.Application.Common;
using Denkraum.Application.Configuration;
using Denkraum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Denkraum.Application.Documents;

public sealed class DocumentService(
    DbContext dbContext,
    IDocumentIndexer documentIndexer,
    IOptions<IndexingOptions> indexingOptions) : IDocumentService
{
    public async Task<Result<IReadOnlyCollection<DocumentDto>>> GetDocumentsAsync(CancellationToken cancellationToken)
    {
        var documents = await dbContext.Set<Document>()
            .AsNoTracking()
            .OrderBy(document => document.FileName)
            .Select(document => new DocumentDto(document.Id, document.FileName, document.FilePath, document.IndexedAt, document.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<DocumentDto>>.Success(documents);
    }

    public async Task<Result<DocumentDto>> GetDocumentAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await dbContext.Set<Document>()
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new DocumentDto(item.Id, item.FileName, item.FilePath, item.IndexedAt, item.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return document is null
            ? Result<DocumentDto>.Failure("Document was not found.")
            : Result<DocumentDto>.Success(document);
    }

    public async Task<Result> IndexDocumentsAsync(CancellationToken cancellationToken)
    {
        var directory = Path.GetFullPath(indexingOptions.Value.DataDirectory);
        return await documentIndexer.IndexDirectoryAsync(directory, cancellationToken);
    }

    public async Task<Result> DeleteDocumentAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await dbContext.Set<Document>().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (document is null)
        {
            return Result.Failure("Document was not found.");
        }

        dbContext.Remove(document);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
