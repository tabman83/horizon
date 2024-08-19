using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using ErrorOr;
using Horizon.Application.AzureKeyVault;
using Horizon.Infrastructure.AzureKeyVault;
using Horizon.Infrastructure.Kubernetes;
using Microsoft.Extensions.Logging;

namespace Horizon.Infrastructure;

internal record SecretStoreKey(string AzureKeyVaultSecretName, string VaultName)
{
    public string AzureKeyVaultSecretName { get; init; } = AzureKeyVaultSecretName.ToLowerInvariant();
    public string VaultName { get; init; } = VaultName.ToLowerInvariant();
}
internal record SecretStoreValue(string Namespace, string Version, string KubernetesSecretName);

public class SecretStore(
    SecretClientFactory secretClientFactory,
    IKubernetesSecretWriter kubernetesSecretWriter,
    ILogger<SecretStore> logger) : ISecretStore
{
    private readonly ConcurrentDictionary<SecretStoreKey, SecretStoreValue> store = new();

    public async Task<ErrorOr<Success>> CopyAzureKeyVaultAsync(string vaultName, string @namespace, string kubernetesSecretName, CancellationToken cancellationToken = default)
    {
        try
        {
            await kubernetesSecretWriter.EnsureExistsAsync(kubernetesSecretName, @namespace, cancellationToken);

            var client = secretClientFactory.CreateClient(vaultName);
            
            var secretProperties = client
                .GetPropertiesOfSecretsAsync(cancellationToken)
                .WithCancellation(cancellationToken);

            await foreach (var secretProperty in secretProperties)
            {
                logger.LogInformation("SecretProperties: {Name} {Enabled} {Version}", secretProperty.Name, secretProperty.Enabled, secretProperty.Version);
                if (secretProperty?.Enabled ?? false)
                {
                    var secretResponse = await client.GetSecretAsync(secretProperty.Name, cancellationToken: cancellationToken);
                    var secret = secretResponse.Value;

                    var key = new SecretStoreKey(secretProperty.Name, vaultName);
                    var value = new SecretStoreValue(@namespace, secret.Properties.Version, kubernetesSecretName);

                    await kubernetesSecretWriter.PatchAsync(kubernetesSecretName, secret.Name, @namespace, secret.Value, cancellationToken);

                    store.AddOrUpdate(key, value, (_, _) => value);
                }
            }

            return Result.Success;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ErrorLoadingAzureKeyVault");
            return Error.Unexpected(description: exception.Message);
        }
    }

    public async Task<ErrorOr<Success>> UpdateSecretAsync(string kvName, string kvSecretName, string kvSecretVersion, CancellationToken cancellationToken = default)
    {
        var key = new SecretStoreKey(kvSecretName, kvName);
        if (!store.TryGetValue(key, out SecretStoreValue? value))
        {
            logger.LogWarning("SecretNotFoundInStore {SecretName} {VaultName}", kvSecretName, kvName);
            return Error.NotFound(description: "Secret not found in store");
        }
        var response = await secretClientFactory
            .CreateClient(kvName)
            .GetSecretAsync(kvSecretName, cancellationToken: cancellationToken);
        var keyVaultSecret = response.Value;
        return Result.Success;
    }
}