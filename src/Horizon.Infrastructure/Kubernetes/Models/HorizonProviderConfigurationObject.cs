using Horizon.Application.Kubernetes;

namespace Horizon.Infrastructure.Kubernetes.Models;

public sealed class HorizonProviderConfigurationObject : HorizonBaseKubernetesObject<HorizonProviderConfigurationSpec>
{
    public override string Plural => "horizonproviderconfigurations";
}