using k8s;
using Horizon.Application.Kubernetes;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon.Infrastructure.Kubernetes;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKubernetes(this IServiceCollection services)
    {
        var config = GetKubernetesConfiguration();
        services.AddSingleton<IKubernetes>(new k8s.Kubernetes(config));
        services.AddTransient<IKubernetesWatcher, KubernetesWatcher>();
        services.AddTransient<IKubernetesSecretWriter, KubernetesSecretWriter>();
        return services;
    }

    private static KubernetesClientConfiguration GetKubernetesConfiguration()
    {
        if (KubernetesClientConfiguration.IsInCluster())
        {
            return KubernetesClientConfiguration.InClusterConfig();
        }
        else
        {
            return KubernetesClientConfiguration.BuildConfigFromConfigFile();
        }
    }
}
