using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace Horizon.Application.Kubernetes
{
    public interface IKubernetesSecretWriter
    {
        Task<ErrorOr<Success>> PatchAsync(string kubernetesSecretName, string @namespace, SecretBundle secret, CancellationToken cancellationToken = default);
        Task<ErrorOr<Success>> ReplaceAsync(string kubernetesSecretObjectName, string @namespace, IEnumerable<SecretBundle> secrets, CancellationToken cancellationToken = default);
    }
}