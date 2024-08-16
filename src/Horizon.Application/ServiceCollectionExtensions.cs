using Horizon.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, AzureKeyVaultSubscriptionAddedResponse>, AzureKeyVaultSubscriptionAddedHandler>();
        return services;
    }
}
