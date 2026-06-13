using Denkraum.Application.Common;

namespace Denkraum.Application.Search;

public sealed record SearchRequest(string Query, int TopK = 5);

public sealed record SearchResponse(IReadOnlyCollection<SearchResult> Results);
