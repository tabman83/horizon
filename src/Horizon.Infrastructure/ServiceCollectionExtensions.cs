using Horizon.Application.Kubernetes;
using Horizon.Application.UseCases;
using k8s;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
    {
        services.AddSingleton<IKubernetes>(new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigFile("C:\\Users\\aparisi\\.kube\\config", "lz-nonprod-we-aks")));
        services.AddTransient<IKubernetesWatcher, KubernetesWatcher>();
        return services;
    }
}
