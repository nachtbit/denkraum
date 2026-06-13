using Denkraum.Application.Abstractions;
using Denkraum.Application.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Denkraum.Infrastructure.HostedServices;

public sealed class DocumentIndexingHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<IndexingOptions> options,
    ILogger<DocumentIndexingHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var directory = Path.GetFullPath(options.Value.DataDirectory);
        logger.LogInformation("Scanning {Directory} for Excel workbooks on startup", directory);
        var indexer = scope.ServiceProvider.GetRequiredService<IDocumentIndexer>();
        var result = await indexer.IndexDirectoryAsync(directory, stoppingToken);
        if (result.IsFailure)
        {
            logger.LogWarning("Startup indexing failed: {Error}", result.Error);
        }
    }
}
