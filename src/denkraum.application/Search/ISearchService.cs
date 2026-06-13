using Denkraum.Application.Common;

namespace Denkraum.Application.Search;

public interface ISearchService
{
    Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
}
