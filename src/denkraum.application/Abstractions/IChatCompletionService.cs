namespace Denkraum.Application.Abstractions;

public interface IChatCompletionService
{
    Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken);
}
