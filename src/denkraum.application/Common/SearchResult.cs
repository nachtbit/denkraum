namespace Denkraum.Application.Common;

public sealed record SearchResult(
    Guid ChunkId,
    string Document,
    string Sheet,
    int Row,
    string Content,
    double Score);
