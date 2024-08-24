namespace Horizon.Application;

public record KubernetesBundle(string KubernetesSecretName, string? SecretPrefix, string Namespace);
