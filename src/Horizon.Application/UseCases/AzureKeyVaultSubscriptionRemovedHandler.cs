using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using Horizon.Application.Kubernetes;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSubscriptionRemovedRequest(IEnumerable<AzureKeyVaultMapping> AzureKeyVaults, string K8sSecretObjectName, string Namespace) : IRequest<ErrorOr<Success>>;

public class AzureKeyVaultSubscriptionRemovedHandler(
    SubscriptionsStore store,
    IKubernetesSecretWriter secretWriter) : IAsyncRequestHandler<AzureKeyVaultSubscriptionRemovedRequest, Success>
{
    internal static Action<Success> EmptyAction => _ => { };

    public async Task<ErrorOr<Success>> HandleAsync(AzureKeyVaultSubscriptionRemovedRequest request, CancellationToken cancellationToken = default)
    {
        List<Error> errors = [];
        foreach (var azureKeyVault in request.AzureKeyVaults)
        {
            store.RemoveSubscription(azureKeyVault.AzureKeyVaultName, new KubernetesBundle(request.K8sSecretObjectName, azureKeyVault.SecretPrefix, request.Namespace))
                .Switch(EmptyAction, errors.AddRange);
        }
        if (errors.Count > 0)
        {
            return errors;
        }
        return await secretWriter.ReplaceAsync(request.K8sSecretObjectName, request.Namespace, [], cancellationToken);
    }
}
