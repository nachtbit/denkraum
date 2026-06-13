namespace Denkraum.Application.Abstractions;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<float[]>> GenerateEmbeddingsAsync(IReadOnlyCollection<string> inputs, CancellationToken cancellationToken);
}
