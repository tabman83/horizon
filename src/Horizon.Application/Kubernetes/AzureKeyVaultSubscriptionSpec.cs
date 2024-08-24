using System.Collections.Generic;

namespace Horizon.Application.Kubernetes;

public record AzureKeyVaultSubscriptionSpec(IEnumerable<AzureKeyVaultSubscription> Vaults);

public record AzureKeyVaultSubscription(string AzureKeyVaultName, string K8sSecretObjectName, string SecretPrefix);