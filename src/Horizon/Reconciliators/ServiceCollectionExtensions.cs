using Horizon.Infrastructure.Kubernetes.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon.Reconciliators;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReconciliators(this IServiceCollection services)
    {
        services.AddSingleton<IReconciliator<AzureKeyVaultSubscriptionObject>, SubscriptionReconciliator>();
        services.AddSingleton<IReconciliator<HorizonProviderConfigurationObject>, ConfigurationReconciliator>();
        return services;
    }
}