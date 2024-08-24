namespace Horizon.Application.Kubernetes.Models;

public interface IHorizonBaseKubernetesObject<TSpec>
{
    public const string Group = "horizon.ninoparisi.io";
    public const string Version = "v1";
    public string Plural { get; }
    TSpec? Spec { get; set; }
}