using System;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Horizon.Infrastructure.AzureKeyVault;
using Microsoft.Extensions.Logging;

namespace Horizon.Infrastructure;

public class SecretStore(
    SecretClientFactory secretClientFactory,
    ILogger<SecretStore> logger) : ISecretStore
{
    public async Task LoadAzureKeyVaultAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        var secretProperties = secretClientFactory
            .CreateClient(uri)
            .GetPropertiesOfSecretsAsync(cancellationToken)
            .WithCancellation(cancellationToken);

        await foreach (var secretProperty in secretProperties)
        {
            logger.LogInformation("SecretProperties: {Name} {Enabled} {Version}", secretProperty.Name, secretProperty.Enabled, secretProperty.Version);
        }
    }
}
