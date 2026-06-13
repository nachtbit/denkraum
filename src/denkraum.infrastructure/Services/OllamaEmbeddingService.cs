using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Denkraum.Application.Abstractions;
using Denkraum.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Denkraum.Infrastructure.Services;

public sealed class OllamaEmbeddingService(
    HttpClient httpClient,
    IOptions<OllamaOptions> options,
    ILogger<OllamaEmbeddingService> logger) : IEmbeddingService
{
    public async Task<float[]> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken)
    {
        var embeddings = await GenerateEmbeddingsAsync([input], cancellationToken);
        return embeddings.First();
    }

    public async Task<IReadOnlyCollection<float[]>> GenerateEmbeddingsAsync(IReadOnlyCollection<string> inputs, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating {Count} embeddings with model {Model}", inputs.Count, options.Value.EmbeddingModel);
        var request = new EmbedRequest(options.Value.EmbeddingModel, inputs);
        using var response = await httpClient.PostAsJsonAsync("/api/embed", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken);
        return payload?.Embeddings ?? throw new InvalidOperationException("Ollama embedding response did not contain embeddings.");
    }

    private sealed record EmbedRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] IReadOnlyCollection<string> Input);

    private sealed record EmbedResponse([property: JsonPropertyName("embeddings")] float[][] Embeddings);
}
