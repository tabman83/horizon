using System;
using ErrorOr;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Microsoft.Extensions.Logging;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSubscriptionAddedRequest(IEnumerable<AzureKeyVaultMappingRequest> Mappings, string Namespace) : IRequest<ErrorOr<AzureKeyVaultSubscriptionAddedResponse>>;

public sealed record AzureKeyVaultMappingRequest(string AzureKeyVaultUrl, string K8sSecretObjectName);

public sealed record AzureKeyVaultSubscriptionAddedResponse();

public class AzureKeyVaultSubscriptionAddedHandler(
    ILogger<AzureKeyVaultSubscriptionAddedHandler> logger,
    ISecretStore secretStore) : IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, ErrorOr<AzureKeyVaultSubscriptionAddedResponse>>
{
    internal static readonly Action<Success> EmptyAction = _ => { };

    public async Task<ErrorOr<AzureKeyVaultSubscriptionAddedResponse>> HandleAsync(AzureKeyVaultSubscriptionAddedRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("HandlingAzureKeyVaultSubscriptionAdded");
        var errorList = new List<Error>();
        foreach (var mapping in request.Mappings)
        {
            var result = await secretStore.MapAzureKeyVaultAsync(new Uri(mapping.AzureKeyVaultUrl), request.Namespace, mapping.K8sSecretObjectName, cancellationToken);
            result.Switch(EmptyAction, errorList.AddRange);
        }
        if (errorList.Count > 0)
        {
            return errorList;
        }
        return new AzureKeyVaultSubscriptionAddedResponse();
    }
}
