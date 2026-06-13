namespace Denkraum.Application.Abstractions;

public interface IFileWatcherService
{
    void Start(CancellationToken cancellationToken);
}
