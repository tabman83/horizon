﻿namespace Horizon.Application.Kubernetes;
public record AzureKeyVaultSubscriptionSpec(string AzureKeyVaultUrl, string K8sSecretObjectName);