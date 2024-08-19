using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace Horizon.Infrastructure.Kubernetes
{
    public interface IKubernetesSecretWriter
    {
        Task<ErrorOr<Success>> PatchAsync(string kubernetesSecretName, string secretName, string @namespace, string secretValue, CancellationToken cancellationToken);
        Task<ErrorOr<Success>> EnsureExistsAsync(string kubernetesSecretName, string @namespace, CancellationToken cancellationToken);
    }
}