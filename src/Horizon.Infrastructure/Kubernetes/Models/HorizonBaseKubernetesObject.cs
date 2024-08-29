using Horizon.Application.Kubernetes;
using k8s.Models;

namespace Horizon.Infrastructure.Kubernetes.Models;

public abstract class HorizonBaseKubernetesObject<TSpec> : IHorizonBaseKubernetesObject<TSpec>
{
    public V1ObjectMeta? Metadata { get; set; }

    public TSpec? Spec { get; set; }

    public abstract string Plural { get; }
}
