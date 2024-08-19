using Azure.Core;
using Azure.Identity;
using Horizon.Application;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;
using Horizon.Infrastructure.AzureKeyVault;
using Horizon.Infrastructure.Kubernetes;
using k8s;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
    {
        services.AddSingleton<IKubernetes>(new k8s.Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigFile("C:\\Users\\aparisi\\.kube\\config", "lz-nonprod-we-aks")));
        services.AddTransient<IKubernetesWatcher, KubernetesWatcher>();
        services.AddTransient<IKubernetesSecretWriter, KubernetesSecretWriter>();
        services.AddSingleton<ISubscriptionsStore, SubscriptionsStore>();
        services.AddSingleton<SecretClientFactory>();
        services.AddTransient<IKeyVaultSecretReader, KeyVaultSecretReader>();
        services.AddSingleton<TokenCredential, DefaultAzureCredential>();
        return services;
    }
}
