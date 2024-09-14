﻿using System;
using ErrorOr;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSubscriptionAddedRequest(IEnumerable<AzureKeyVaultMapping> Mappings, string Namespace) : IRequest<ErrorOr<Success>>;

public class AzureKeyVaultSubscriptionAddedHandler(
    IKeyVaultSecretReader secretReader,
    ISubscriptionsStore store,
    IKubernetesSecretWriter secretWriter) : IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, Success>
{
    internal static Action<Success> EmptyAction => _ => { };

    public async Task<ErrorOr<Success>> HandleAsync(AzureKeyVaultSubscriptionAddedRequest request, CancellationToken cancellationToken = default)
    {
        List<Error> errorList = [];
        foreach (var mapping in request.Mappings)
        {
            await store.AddSubscription(mapping.AzureKeyVaultName, new KubernetesBundle(mapping.K8sSecretObjectName, mapping.SecretPrefix, request.Namespace))
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
