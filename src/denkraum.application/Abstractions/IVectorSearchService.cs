using Denkraum.Application.Common;

namespace Denkraum.Application.Abstractions;

public interface IVectorSearchService
{
    Task<IReadOnlyCollection<SearchResult>> SearchAsync(string query, int topK, CancellationToken cancellationToken);
}
