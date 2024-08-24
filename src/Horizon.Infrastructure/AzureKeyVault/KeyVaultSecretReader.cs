using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using ErrorOr;
using Horizon.Application;
using Horizon.Application.AzureKeyVault;
using Microsoft.Extensions.Logging;

namespace Horizon.Infrastructure.AzureKeyVault;


public class KeyVaultSecretReader(
    ILogger<KeyVaultSecretReader> logger,
    SecretClientFactory clientFactory) : IKeyVaultSecretReader
{
    public async Task<ErrorOr<SecretBundle>> LoadSingleSecretAsync(string vaultName, string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = clientFactory.CreateClient(vaultName);
            var secretResponse = await client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            var secret = secretResponse.Value;
            return new SecretBundle(secret.Name, secret.Value);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ErrorLoadingSecret");
            return Error.Unexpected(description: exception.Message);
        }
    }

    public async Task<ErrorOr<IEnumerable<SecretBundle>>> LoadAllSecretsAsync(string vaultName, string? secretPrefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = clientFactory.CreateClient(vaultName);
            var secretPages = client.GetPropertiesOfSecretsAsync(cancellationToken);

            using var secretLoader = new ParallelSecretLoader(client);
            await foreach (var secret in secretPages)
            {
                if (secret.Enabled != true || 
                    (secretPrefix is not null && !secret.Name.StartsWith(secretPrefix, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                secretLoader.AddSecretToLoad(secret.Name);
            }
            var loadedSecrets = await secretLoader.WaitForAllAsync();
            var secretBundles = loadedSecrets.Select(x => new SecretBundle(x.Value.Name, x.Value.Value));
            return secretBundles.ToErrorOr();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ErrorLoadingSecrets");
            return Error.Unexpected(description: exception.Message);
        }
    }
}
