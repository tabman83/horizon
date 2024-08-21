using k8s.Models;
using k8s;
using System.Collections.Generic;
using Horizon.Application.Kubernetes;

namespace Horizon.Infrastructure.Kubernetes.Models;

public sealed class AzureKeyVaultSubscriptionObject : KubernetesObject
{
    public const string Group = "horizon.ninoparisi.io";
    public const string Version = "v1";
    public const string Plural = "azurekeyvaultsubscriptions";

    public V1ObjectMeta? Metadata { get; set; }
    public IEnumerable<AzureKeyVaultSubscriptionSpec> Spec { get; set; } = [];
}