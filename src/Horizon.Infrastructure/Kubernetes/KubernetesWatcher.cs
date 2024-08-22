using System.Threading.Tasks;
using System.Threading;
using k8s;
using Horizon.Application.Kubernetes;
using System;
using Microsoft.Extensions.Logging;
using Horizon.Infrastructure.Kubernetes.Models;

namespace Horizon.Infrastructure.Kubernetes;

public class KubernetesWatcher(
    IKubernetes client,
    ILogger<KubernetesWatcher> logger) : IKubernetesWatcher
{
    public async Task RunWatcherAsync(ReconcileDelegate reconcileDelegate, CancellationToken cancellationToken = default)
    {
        var watch = client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync<AzureKeyVaultSubscriptionObject>(
            group: AzureKeyVaultSubscriptionObject.Group,
            version: AzureKeyVaultSubscriptionObject.Version,
            plural: AzureKeyVaultSubscriptionObject.Plural,
            watch: true,
            cancellationToken: cancellationToken);
        
        await foreach (var (type, item) in watch.WatchAsync<AzureKeyVaultSubscriptionObject, AzureKeyVaultSubscriptionObject>(cancellationToken: cancellationToken))
        {
            logger.LogInformation("WatchingCustomObject");
            await reconcileDelegate((Application.Kubernetes.WatchEventType)type, item.Metadata?.Name, item.Metadata?.NamespaceProperty, item.Spec);
        }
    }
}
