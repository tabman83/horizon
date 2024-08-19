using ErrorOr;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Microsoft.Extensions.Logging;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSecretNewVersionCreatedRequest(string VaultName, string SecretName, string Version) : IRequest<ErrorOr<Success>>;

public class AzureKeyVaultSecretNewVersionCreatedHandler(
    ILogger<AzureKeyVaultSecretNewVersionCreatedHandler> logger,
    ISecretStore secretStore) : IAsyncRequestHandler<AzureKeyVaultSecretNewVersionCreatedRequest, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> HandleAsync(AzureKeyVaultSecretNewVersionCreatedRequest request, CancellationToken cancellationToken = default)
    {
        return await secretStore.UpdateSecretAsync(request.VaultName, request.SecretName, request.Version, cancellationToken);
    }
}
