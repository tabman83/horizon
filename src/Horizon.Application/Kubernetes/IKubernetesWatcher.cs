using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Horizon.Application.Kubernetes
{
    public interface IKubernetesWatcher
    {
        Task RunWatcherAsync(Action<WatchEventType, IEnumerable<AzureKeyVaultSubscriptionSpec>> watchAction, CancellationToken cancellationToken = default);
    }
}