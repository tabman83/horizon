using System.Collections.Generic;
using ErrorOr;
using Horizon.Application;

namespace Horizon.Application
{
    public interface ISubscriptionsStore
    {
        ErrorOr<Success> AddSubscription(string azureKeyVaultName, KubernetesBundle kubernetesBundle);
        ErrorOr<Success> RemoveSubscription(string azureKeyVaultName, KubernetesBundle kubernetesBundle);
        ErrorOr<IEnumerable<KubernetesBundle>> GetSubscription(string azureKeyVaultName);
    }
}