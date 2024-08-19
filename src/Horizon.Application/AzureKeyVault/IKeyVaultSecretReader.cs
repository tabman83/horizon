using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace Horizon.Application.AzureKeyVault
{
    public interface IKeyVaultSecretReader
    {
        Task<ErrorOr<IEnumerable<SecretBundle>>> LoadAllSecretsAsync(string vaultName, CancellationToken cancellationToken = default);
        Task<ErrorOr<SecretBundle>> LoadSingleSecretAsync(string vaultName, string secretName, CancellationToken cancellationToken = default);
    }
}