using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Microsoft.Extensions.Logging;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSubscriptionAddedRequest(IEnumerable<AzureKeyVaultMappingRequest> Mappings) : IRequest<AzureKeyVaultSubscriptionAddedResponse>;

public sealed record AzureKeyVaultMappingRequest(string AzureKeyVaultUrl, string K8sSecretObjectName);

public sealed record AzureKeyVaultSubscriptionAddedResponse();

public class AzureKeyVaultSubscriptionAddedHandler(
    ILogger<AzureKeyVaultSubscriptionAddedHandler> logger,
    ISecretStore secretStore) : IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, AzureKeyVaultSubscriptionAddedResponse>
{
    public async Task<AzureKeyVaultSubscriptionAddedResponse> HandleAsync(AzureKeyVaultSubscriptionAddedRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("HandlingAzureKeyVaultSubscriptionAdded");
        foreach(var mapping in request.Mappings)
        {
            await secretStore.LoadAzureKeyVaultAsync(new Uri(mapping.AzureKeyVaultUrl), cancellationToken);
        }
        // Simulate async work
        await Task.Delay(100);
        return new AzureKeyVaultSubscriptionAddedResponse();
    }
}
