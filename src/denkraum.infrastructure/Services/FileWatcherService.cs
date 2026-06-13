using System.Threading.Channels;
using Denkraum.Application.Abstractions;
using Denkraum.Application.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Denkraum.Infrastructure.Services;

public sealed class FileWatcherService(
    IServiceScopeFactory scopeFactory,
    IOptions<IndexingOptions> options,
    ILogger<FileWatcherService> logger) : BackgroundService, IFileWatcherService
{
    private readonly Channel<FileChange> _queue = Channel.CreateUnbounded<FileChange>();
    private readonly HashSet<string> _pending = new(StringComparer.OrdinalIgnoreCase);
    private FileSystemWatcher? _watcher;

    public void Start(CancellationToken cancellationToken) => _ = StartAsync(cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var directory = Path.GetFullPath(options.Value.DataDirectory);
        Directory.CreateDirectory(directory);

        _watcher = new FileSystemWatcher(directory, "*.xlsx")
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
        };
        _watcher.Created += (_, args) => Queue(args.FullPath, FileChangeKind.Upsert);
        _watcher.Changed += (_, args) => Queue(args.FullPath, FileChangeKind.Upsert);
        _watcher.Deleted += (_, args) => Queue(args.FullPath, FileChangeKind.Delete);
        _watcher.Renamed += (_, args) =>
        {
            Queue(args.OldFullPath, FileChangeKind.Delete);
            Queue(args.FullPath, FileChangeKind.Upsert);
        };

        logger.LogInformation("Watching {Directory} for Excel workbook changes", directory);

        await foreach (var change in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            lock (_pending)
            {
                _pending.Remove(change.Path);
            }

            using var scope = scopeFactory.CreateScope();
            var indexer = scope.ServiceProvider.GetRequiredService<IDocumentIndexer>();
            var result = change.Kind == FileChangeKind.Delete
                ? await indexer.DeleteByPathAsync(change.Path, stoppingToken)
                : await indexer.IndexFileAsync(change.Path, stoppingToken);

            if (result.IsFailure)
            {
                logger.LogWarning("File watcher operation failed for {Path}: {Error}", change.Path, result.Error);
            }
        }
    }

    public override void Dispose()
    {
        _watcher?.Dispose();
        base.Dispose();
    }

    private void Queue(string path, FileChangeKind kind)
    {
        if (!path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        lock (_pending)
        {
            if (!_pending.Add(path))
            {
                return;
            }
        }

        _queue.Writer.TryWrite(new FileChange(path, kind));
    }

    private sealed record FileChange(string Path, FileChangeKind Kind);

    private enum FileChangeKind
    {
        Upsert,
        Delete
    }
}
