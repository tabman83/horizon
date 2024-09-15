using ErrorOr;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSubscriptionAddedRequest(IEnumerable<AzureKeyVaultMapping> AzureKeyVaults, string K8sSecretObjectName, string Namespace) : IRequest<ErrorOr<Success>>;

public class AzureKeyVaultSubscriptionAddedHandler(
    IKeyVaultSecretReader secretReader,
    SubscriptionsStore store,
    IKubernetesSecretWriter secretWriter) : IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, Success>
{
    public async Task<ErrorOr<Success>> HandleAsync(AzureKeyVaultSubscriptionAddedRequest request, CancellationToken cancellationToken = default)
    {
        List<Error> errors = [];
        List<SecretBundle> secretBundles = [];
        foreach (var azureKeyVault in request.AzureKeyVaults)
        {
            await store.AddSubscription(azureKeyVault.AzureKeyVaultName, new KubernetesBundle(request.K8sSecretObjectName, azureKeyVault.SecretPrefix, request.Namespace))
                .ThenAsync(_ => secretReader.LoadAllSecretsAsync(azureKeyVault.AzureKeyVaultName, azureKeyVault.SecretPrefix, cancellationToken))
                .Switch(secretBundles.AddRange, errors.AddRange);
        }
        if (errors.Count > 0)
        {
            return errors;
        }
        return await secretWriter.ReplaceAsync(request.K8sSecretObjectName, request.Namespace, secretBundles, cancellationToken);
    }
}
