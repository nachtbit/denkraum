using Denkraum.Application.Common;

namespace Denkraum.Application.Documents;

public interface IDocumentService
{
    Task<Result<IReadOnlyCollection<DocumentDto>>> GetDocumentsAsync(CancellationToken cancellationToken);
    Task<Result<DocumentDto>> GetDocumentAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> IndexDocumentsAsync(CancellationToken cancellationToken);
    Task<Result> DeleteDocumentAsync(Guid id, CancellationToken cancellationToken);
}
