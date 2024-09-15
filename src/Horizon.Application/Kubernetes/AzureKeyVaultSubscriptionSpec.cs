using System.Collections.Generic;

namespace Horizon.Application.Kubernetes;

public record AzureKeyVaultSubscriptionSpec(string K8sSecretObjectName, IEnumerable<AzureKeyVaultSubscription> Vaults);

public record AzureKeyVaultSubscription(string AzureKeyVaultName, string SecretPrefix);