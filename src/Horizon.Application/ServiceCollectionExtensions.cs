using ErrorOr;
using Horizon.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, Success>, AzureKeyVaultSubscriptionAddedHandler>();
        services.AddTransient<IAsyncRequestHandler<AzureKeyVaultSecretNewVersionCreatedRequest, Success>, AzureKeyVaultSecretNewVersionCreatedHandler>();
        services.AddTransient<IAsyncRequestHandler<AzureKeyVaultSubscriptionRemovedRequest, Success>, AzureKeyVaultSubscriptionRemovedHandler>();
        return services;
    }
}
