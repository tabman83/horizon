using System;
using ErrorOr;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Microsoft.Extensions.Logging;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSubscriptionAddedRequest(IEnumerable<AzureKeyVaultMappingRequest> Mappings, string Namespace) : IRequest<ErrorOr<Success>>;

public sealed record AzureKeyVaultMappingRequest(string AzureKeyVaultName, string K8sSecretObjectName)
{
    public string AzureKeyVaultName { get; init; } = AzureKeyVaultName.ToLowerInvariant();
}

public class AzureKeyVaultSubscriptionAddedHandler(
    ILogger<AzureKeyVaultSubscriptionAddedHandler> logger,
    ISecretStore secretStore) : IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, ErrorOr<Success>>
{
    internal static readonly Action<Success> EmptyAction = _ => { };

    public async Task<ErrorOr<Success>> HandleAsync(AzureKeyVaultSubscriptionAddedRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("HandlingAzureKeyVaultSubscriptionAdded");
        List<Error> errorList = [];
        foreach (var mapping in request.Mappings)
        {
            var result = await secretStore.CopyAzureKeyVaultAsync(mapping.AzureKeyVaultName, request.Namespace, mapping.K8sSecretObjectName, cancellationToken);
            result.Switch(EmptyAction, errorList.AddRange);
        }
        if (errorList.Count > 0)
        {
            return errorList;
        }
        return Result.Success;
    }
}
