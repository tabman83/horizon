using System.Threading.Tasks;
using System.Threading;
using k8s;
using Horizon.Application.Kubernetes;
using Microsoft.Extensions.Logging;
using Horizon.Application.Kubernetes.Models;

namespace Horizon.Infrastructure.Kubernetes;

public class KubernetesWatcher(
    IKubernetes client,
    ILogger<KubernetesWatcher> logger) : IKubernetesWatcher
{
    public async Task WatchAsync<T, TSpec>(ReconcileDelegate<T> reconcileDelegate, CancellationToken cancellationToken = default)
        where T : IHorizonBaseKubernetesObject<TSpec>, new()
    {
        var watch = client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync<T>(
            group: IHorizonBaseKubernetesObject<TSpec>.Group,
            version: IHorizonBaseKubernetesObject<TSpec>.Version,
            plural: new T().Plural,
            watch: true,
            cancellationToken: cancellationToken);
        
        await foreach (var (type, item) in watch.WatchAsync<T, T>(cancellationToken: cancellationToken))
        {
            logger.LogInformation("WatchingCustomObject");
            await reconcileDelegate((Application.Kubernetes.WatchEventType)type, item);
        }
    }
}
