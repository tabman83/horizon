namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultMapping(string AzureKeyVaultName, string K8sSecretObjectName, string? SecretPrefix);
