﻿namespace Horizon.Application.Kubernetes;
public record AzureKeyVaultSubscriptionSpec(string AzureKeyVaultName, string K8sSecretObjectName);