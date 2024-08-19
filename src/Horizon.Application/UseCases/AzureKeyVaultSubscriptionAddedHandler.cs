using System;
using ErrorOr;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Microsoft.Extensions.Logging;
using Horizon.Application.Kubernetes;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSubscriptionAddedRequest(IEnumerable<AzureKeyVaultMappingRequest> Mappings, string Namespace) : IRequest<ErrorOr<Success>>;

public sealed record AzureKeyVaultMappingRequest(string AzureKeyVaultName, string K8sSecretObjectName)
{
    public string AzureKeyVaultName { get; init; } = AzureKeyVaultName.ToLowerInvariant();
}

public class AzureKeyVaultSubscriptionAddedHandler(
    ILogger<AzureKeyVaultSubscriptionAddedHandler> logger,
    IKeyVaultSecretReader secretReader,
    ISubscriptionsStore store,
    IKubernetesSecretWriter secretWriter) : IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, ErrorOr<Success>>
{
    internal static Action<Success> EmptyAction => _ => { };

    public async Task<ErrorOr<Success>> HandleAsync(AzureKeyVaultSubscriptionAddedRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("HandlingAzureKeyVaultSubscriptionAdded");
        List<Error> errorList = [];
        foreach (var mapping in request.Mappings)
        {
            await store.AddSubscription(mapping.AzureKeyVaultName, new KubernetesBundle(mapping.K8sSecretObjectName, request.Namespace))
                .ThenAsync(_ => secretReader.LoadAllSecretsAsync(mapping.AzureKeyVaultName, cancellationToken))
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
