using ErrorOr;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Microsoft.Extensions.Logging;
using Horizon.Application.Kubernetes;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSecretNewVersionCreatedRequest(string VaultName, string SecretName, string Version) : IRequest<ErrorOr<Success>>;

public class AzureKeyVaultSecretNewVersionCreatedHandler(
    ILogger<AzureKeyVaultSecretNewVersionCreatedHandler> logger,
    IKeyVaultSecretReader secretReader,
    IKubernetesSecretWriter secretWriter) : IAsyncRequestHandler<AzureKeyVaultSecretNewVersionCreatedRequest, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> HandleAsync(AzureKeyVaultSecretNewVersionCreatedRequest request, CancellationToken cancellationToken = default)
    {
        return await secretReader.LoadSingleSecretAsync(request.VaultName, request.SecretName, cancellationToken)
            .ThenAsync(secret => secretWriter.PatchAsync(request.SecretName, request.Namespace, secret, cancellationToken));
    }
}
