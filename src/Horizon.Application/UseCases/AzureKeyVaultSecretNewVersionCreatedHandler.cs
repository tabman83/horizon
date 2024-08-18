using ErrorOr;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Microsoft.Extensions.Logging;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSecretNewVersionCreatedRequest() : IRequest<ErrorOr<AzureKeyVaultSecretNewVersionCreatedResponse>>;

public sealed record AzureKeyVaultSecretNewVersionCreatedResponse();

public class AzureKeyVaultSecretNewVersionCreatedHandler(
    ILogger<AzureKeyVaultSecretNewVersionCreatedHandler> logger,
    ISecretStore secretStore) : IAsyncRequestHandler<AzureKeyVaultSecretNewVersionCreatedRequest, ErrorOr<AzureKeyVaultSecretNewVersionCreatedResponse>>
{
    public Task<ErrorOr<AzureKeyVaultSecretNewVersionCreatedResponse>> HandleAsync(AzureKeyVaultSecretNewVersionCreatedRequest request, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}
