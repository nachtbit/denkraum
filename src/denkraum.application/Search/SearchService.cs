using Denkraum.Application.Abstractions;
using Denkraum.Application.Common;

namespace Denkraum.Application.Search;

public sealed class SearchService(IVectorSearchService vectorSearchService) : ISearchService
{
    public async Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        var results = await vectorSearchService.SearchAsync(request.Query, request.TopK, cancellationToken);
        return Result<SearchResponse>.Success(new SearchResponse(results));
    }
}
