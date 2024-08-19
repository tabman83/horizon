using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    public async Task<ErrorOr<IEnumerable<SecretBundle>>> LoadAllSecretsAsync(string vaultName, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = clientFactory.CreateClient(vaultName);
            var secretProperties = client.GetPropertiesOfSecretsAsync(cancellationToken);
            List<SecretBundle> secrets = [];
            await foreach (var secretProperty in secretProperties)
            {
                if (secretProperty?.Enabled ?? false)
                {
                    var secretResponse = await client.GetSecretAsync(secretProperty.Name, cancellationToken: cancellationToken);
                    var secret = secretResponse.Value;
                    secrets.Add(new SecretBundle(secret.Name, secret.Value));
                }
            }
            return secrets;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ErrorLoadingSecrets");
            return Error.Unexpected(description: exception.Message);
        }
    }
}
