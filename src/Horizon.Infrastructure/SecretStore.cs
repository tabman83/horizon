using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using Horizon.Application.AzureKeyVault;
using Horizon.Infrastructure.AzureKeyVault;
using Microsoft.Extensions.Logging;

namespace Horizon.Infrastructure;

internal record SecretStoreKey(string kvSecretName, Uri kvUri);
internal record SecretStoreValue(string @namespace, DateTimeOffset lastUpdated, string kubernetesSecretName);

public class SecretStore(
    SecretClientFactory secretClientFactory,
    ILogger<SecretStore> logger) : ISecretStore
{
    private readonly ConcurrentDictionary<SecretStoreKey, SecretStoreValue> store = new();

    public async Task<ErrorOr<Success>> MapAzureKeyVaultAsync(Uri uri, string @namespace, string kubernetesSecretName, CancellationToken cancellationToken = default)
    {
        try
        {
            var secretProperties = secretClientFactory
                .CreateClient(uri)
                .GetPropertiesOfSecretsAsync(cancellationToken)
                .WithCancellation(cancellationToken);

            List<SecretMetadata> secrets = [];

            await foreach (var secretProperty in secretProperties)
            {
                logger.LogInformation("SecretProperties: {Name} {Enabled} {Version}", secretProperty.Name, secretProperty.Enabled, secretProperty.Version);
                if (secretProperty?.Enabled ?? false)
                {
                    var key = new SecretStoreKey(secretProperty.Name, uri);
                    var value = new SecretStoreValue(@namespace, secretProperty.UpdatedOn ?? DateTimeOffset.MinValue, kubernetesSecretName);
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
}