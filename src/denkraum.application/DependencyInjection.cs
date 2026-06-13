using Denkraum.Application.Chat;
using Denkraum.Application.Dashboard;
using Denkraum.Application.Documents;
using Denkraum.Application.Search;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Denkraum.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IDashboardService, DashboardService>();
        return services;
    }
}
