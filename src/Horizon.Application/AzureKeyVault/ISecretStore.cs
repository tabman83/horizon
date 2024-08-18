using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using ErrorOr;

namespace Horizon.Application.AzureKeyVault;

public interface ISecretStore
{
    public Task<ErrorOr<Success>> MapAzureKeyVaultAsync(Uri uri, string @namespace, string kubernetesSecretName, CancellationToken cancellationToken = default);
}
