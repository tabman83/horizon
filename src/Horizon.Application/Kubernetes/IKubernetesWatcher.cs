﻿using System.Threading;
using System.Threading.Tasks;

namespace Horizon.Application.Kubernetes
{
    public interface IKubernetesWatcher
    {
        Task WatchAsync<T, TSpec>(ReconcileDelegate<T> reconcileDelegate, CancellationToken cancellationToken = default)
            where T : IHorizonBaseKubernetesObject<TSpec>, new();
    }
}