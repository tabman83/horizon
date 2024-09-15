using System;
using Azure.Core;
using Azure.Identity;
using Horizon.Application;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;
using Horizon.Infrastructure.AzureKeyVault;
using Horizon.Infrastructure.Kubernetes;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
    {
        services.AddKubernetes();
        services.AddSingleton<SubscriptionsStore>();
        services.AddSingleton<SecretClientFactory>();
        services.AddTransient<IKeyVaultSecretReader, KeyVaultSecretReader>();
        services.AddSingleton<TokenCredential, DefaultAzureCredential>();
        return services;
    }
}
