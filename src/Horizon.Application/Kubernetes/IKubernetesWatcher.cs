using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Horizon.Application.Kubernetes
{
    public interface IKubernetesWatcher
    {
        Task RunWatcherAsync(ReconcileDelegate reconcileDelegate, CancellationToken cancellationToken = default);
    }
}