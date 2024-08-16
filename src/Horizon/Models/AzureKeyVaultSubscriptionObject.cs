using k8s.Models;
using k8s;
using System.Collections.Generic;

namespace Horizon.Models;

public sealed class AzureKeyVaultSubscriptionObject : KubernetesObject
{
    public const string Group = "horizon.sh";
    public const string Version = "v1";
    public const string Plural = "azurekeyvaultsubscriptions";

    public V1ObjectMeta? Metadata { get; set; }
    public IEnumerable<AzureKeyVaultSubscriptionSpec> Spec { get; set; } = [];

    public record AzureKeyVaultSubscriptionSpec(string AzureKeyVaultUrl, string K8sSecretObjectName);
}