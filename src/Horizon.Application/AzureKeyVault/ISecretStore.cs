using System.Threading.Tasks;
using System.Threading;
using System;

namespace Horizon.Application.AzureKeyVault;

public interface ISecretStore
{
    public Task LoadAzureKeyVaultAsync(Uri uri, CancellationToken cancellationToken = default);
}
