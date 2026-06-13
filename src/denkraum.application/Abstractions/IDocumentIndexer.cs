using Denkraum.Application.Common;

namespace Denkraum.Application.Abstractions;

public interface IDocumentIndexer
{
    Task<Result> IndexFileAsync(string filePath, CancellationToken cancellationToken);
    Task<Result> IndexDirectoryAsync(string directory, CancellationToken cancellationToken);
    Task<Result> DeleteByPathAsync(string filePath, CancellationToken cancellationToken);
}
