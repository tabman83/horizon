using Horizon.Application.Kubernetes;

namespace Horizon.Infrastructure.Kubernetes.Models;

public sealed class AzureKeyVaultSubscriptionObject : HorizonBaseKubernetesObject<AzureKeyVaultSubscriptionSpec>
{
    public override string Plural => "azurekeyvaultsubscriptions";
}