using System.Threading.Tasks;
using System.Threading;
using ErrorOr;

namespace Horizon.Application.AzureKeyVault;

public interface ISecretStore
{
    public Task<ErrorOr<Success>> CopyAzureKeyVaultAsync(string kvName, string @namespace, string kubernetesSecretName, CancellationToken cancellationToken = default);
    public Task<ErrorOr<Success>> UpdateSecretAsync(string kvName, string kvSecretName, string kvSecretVersion, CancellationToken cancellationToken = default);
}
