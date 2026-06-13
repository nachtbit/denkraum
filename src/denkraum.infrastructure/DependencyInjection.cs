using Denkraum.Application.Abstractions;
using Denkraum.Application.Configuration;
using Denkraum.Infrastructure.HostedServices;
using Denkraum.Infrastructure.Persistence;
using Denkraum.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Pgvector.EntityFrameworkCore;

namespace Denkraum.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OllamaOptions>()
            .Bind(configuration.GetSection(OllamaOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "Ollama:BaseUrl must be an absolute URL.")
            .ValidateOnStart();

        services.AddOptions<IndexingOptions>()
            .Bind(configuration.GetSection(IndexingOptions.SectionName))
            .Validate(options => options.BatchSize > 0, "Indexing:BatchSize must be greater than zero.")
            .ValidateOnStart();

        services.AddDbContext<DenkraumDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreSql"),
                npgsql => npgsql.UseVector()));
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<DenkraumDbContext>());

        services.AddScoped<IExcelDocumentReader, ExcelDocumentReader>();
        services.AddScoped<IVectorSearchService, VectorSearchService>();
        services.AddScoped<IDocumentIndexer, DocumentIndexer>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(ConfigureOllamaClient)
            .AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<IChatCompletionService, OllamaChatCompletionService>(ConfigureOllamaClient)
            .AddPolicyHandler(GetRetryPolicy());

        services.AddHostedService<DocumentIndexingHostedService>();
        services.AddSingleton<FileWatcherService>();
        services.AddSingleton<IFileWatcherService>(provider => provider.GetRequiredService<FileWatcherService>());
        services.AddHostedService(provider => provider.GetRequiredService<FileWatcherService>());

        return services;
    }

    private static void ConfigureOllamaClient(IServiceProvider provider, HttpClient client)
    {
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)));
}
