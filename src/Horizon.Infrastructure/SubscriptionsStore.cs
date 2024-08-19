using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ErrorOr;
using Horizon.Application;

namespace Horizon.Infrastructure;

public class SubscriptionsStore : ISubscriptionsStore
{
    private readonly ConcurrentDictionary<string, IEnumerable<KubernetesBundle>> store = new();

    public ErrorOr<Success> AddSubscription(string azureKeyVaultName, KubernetesBundle kubernetesBundle)
    {
        try
        {
            store.AddOrUpdate(azureKeyVaultName, [kubernetesBundle], (_, bundles) =>
                new List<KubernetesBundle>(bundles) { kubernetesBundle });
            return Result.Success;
        }
        catch (Exception e)
        {
            return Error.Unexpected(description: e.Message);
        }
    }

    public ErrorOr<IEnumerable<KubernetesBundle>> GetSubscription(string azureKeyVaultName)
    {
        if (!store.TryGetValue(azureKeyVaultName, out var kubernetesBundles))
        {
            return Error.NotFound();
        }
        return kubernetesBundles.ToErrorOr();
    }
}
