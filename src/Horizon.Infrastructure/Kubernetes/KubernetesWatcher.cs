using System.Threading.Tasks;
using System.Threading;
using k8s;
using Horizon.Application.Kubernetes;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Horizon.Infrastructure.Kubernetes.Models;

namespace Horizon.Infrastructure.Kubernetes;

public class KubernetesWatcher(
    IKubernetes Client,
    ILogger<KubernetesWatcher> Logger) : IKubernetesWatcher
{
    public async Task RunWatcherAsync(Action<Application.Kubernetes.WatchEventType, IEnumerable<AzureKeyVaultSubscriptionSpec>> watchAction, CancellationToken cancellationToken = default)
    {
        var watch = Client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync<AzureKeyVaultSubscriptionObject>(
            group: AzureKeyVaultSubscriptionObject.Group,
            version: AzureKeyVaultSubscriptionObject.Version,
            plural: AzureKeyVaultSubscriptionObject.Plural,
            watch: true,
            cancellationToken: cancellationToken);

        using (watch.Watch(Reconcile(watchAction)))
        {
            Logger.LogInformation("WatchingCustomObject");
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
    }

    private static Action<k8s.WatchEventType, AzureKeyVaultSubscriptionObject> Reconcile(Action<Application.Kubernetes.WatchEventType, IEnumerable<AzureKeyVaultSubscriptionSpec>> watchAction)
    {
        return (type, @object) => watchAction((Application.Kubernetes.WatchEventType)type, @object.Spec);

    }
}
