using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSubscriptionAddedRequest(IEnumerable<AzureKeyVaultMappingRequest> Mappings) : IRequest<AzureKeyVaultSubscriptionAddedResponse>;

public sealed record AzureKeyVaultMappingRequest(string AzureKeyVaultUrl, string K8sSecretObjectName);

public sealed record AzureKeyVaultSubscriptionAddedResponse();

public class AzureKeyVaultSubscriptionAddedHandler(
    ILogger<AzureKeyVaultSubscriptionAddedHandler> logger) : IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, AzureKeyVaultSubscriptionAddedResponse>
{
    public async Task<AzureKeyVaultSubscriptionAddedResponse> HandleAsync(AzureKeyVaultSubscriptionAddedRequest request)
    {
        logger.LogInformation("HandlingAzureKeyVaultSubscriptionAdded");
        // Simulate async work
        await Task.Delay(100);
        return new AzureKeyVaultSubscriptionAddedResponse();
    }
}
