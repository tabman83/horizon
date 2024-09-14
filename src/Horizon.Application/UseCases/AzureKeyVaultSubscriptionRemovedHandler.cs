using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSubscriptionRemovedRequest(IEnumerable<AzureKeyVaultMapping> Mappings, string Namespace) : IRequest<ErrorOr<Success>>;

public sealed record AzureKeyVaultMappingRemovedRequest(string AzureKeyVaultName, string K8sSecretObjectName, string? SecretPrefix);

public class AzureKeyVaultSubscriptionRemovedHandler(
    IKeyVaultSecretReader secretReader,
    ISubscriptionsStore store,
    IKubernetesSecretWriter secretWriter) : IAsyncRequestHandler<AzureKeyVaultSubscriptionRemovedRequest, Success>
{
    internal static Action<Success> EmptyAction => _ => { };

    public async Task<ErrorOr<Success>> HandleAsync(AzureKeyVaultSubscriptionRemovedRequest request, CancellationToken cancellationToken = default)
    {
        List<Error> errorList = [];
        foreach (var mapping in request.Mappings)
        {
            await store.RemoveSubscription(mapping.AzureKeyVaultName, new KubernetesBundle(mapping.K8sSecretObjectName, mapping.SecretPrefix, request.Namespace))
                .ThenAsync(_ => secretReader.LoadAllSecretsAsync(mapping.AzureKeyVaultName, mapping.SecretPrefix, cancellationToken))
                .ThenAsync(secrets => secretWriter.ReplaceAsync(mapping.K8sSecretObjectName, request.Namespace, secrets, cancellationToken))
                .Switch(EmptyAction, errorList.AddRange);
        }
        if (errorList.Count > 0)
        {
            return errorList;
        }
        return Result.Success;
    }
}
