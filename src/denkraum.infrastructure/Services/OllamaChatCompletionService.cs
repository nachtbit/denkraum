using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Denkraum.Application.Abstractions;
using Denkraum.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Denkraum.Infrastructure.Services;

public sealed class OllamaChatCompletionService(
    HttpClient httpClient,
    IOptions<OllamaOptions> options,
    ILogger<OllamaChatCompletionService> logger) : IChatCompletionService
{
    public async Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken)
    {
        logger.LogInformation("Requesting grounded chat completion with model {Model}", options.Value.ChatModel);
        var request = new ChatRequest(
            options.Value.ChatModel,
            [new ChatMessage("user", prompt)],
            false);

        using var response = await httpClient.PostAsJsonAsync("/api/chat", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken);
        return payload?.Message?.Content?.Trim() ?? throw new InvalidOperationException("Ollama chat response did not contain content.");
    }

    private sealed record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyCollection<ChatMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record ChatResponse([property: JsonPropertyName("message")] ChatMessage? Message);
}
