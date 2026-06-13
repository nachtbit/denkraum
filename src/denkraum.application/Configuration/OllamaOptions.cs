namespace Denkraum.Application.Configuration;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
    public required string ChatModel { get; init; }
    public required string EmbeddingModel { get; init; }
}
