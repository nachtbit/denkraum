namespace Denkraum.Application.Configuration;

public sealed class IndexingOptions
{
    public const string SectionName = "Indexing";

    public string DataDirectory { get; init; } = "data";
    public int BatchSize { get; init; } = 16;
}
