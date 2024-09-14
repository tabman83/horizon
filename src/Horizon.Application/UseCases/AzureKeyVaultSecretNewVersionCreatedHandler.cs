using ErrorOr;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;
using System.Collections.Generic;
using System.Linq;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSecretNewVersionCreatedRequest(string VaultName, string SecretName) : IRequest<ErrorOr<Success>>;

public class AzureKeyVaultSecretNewVersionCreatedHandler(
    ISubscriptionsStore store,
    IKeyVaultSecretReader secretReader,
    IKubernetesSecretWriter secretWriter) : IAsyncRequestHandler<AzureKeyVaultSecretNewVersionCreatedRequest, Success>
{
    public async Task<ErrorOr<Success>> HandleAsync(AzureKeyVaultSecretNewVersionCreatedRequest request, CancellationToken cancellationToken = default)
    {
        return await secretReader.LoadSingleSecretAsync(request.VaultName, request.SecretName, cancellationToken)
            .Then(secret => GetSubscription(secret, request.VaultName))
            .ThenAsync(bundle => PatchSecretsAsync(bundle.KubernetesBundles, bundle.SecretBundle, cancellationToken));
    }

    private ErrorOr<Bundle> GetSubscription(SecretBundle secretBundle, string vaultName)
    {
        return store.GetSubscription(vaultName)
            .Then(subscriptions => new Bundle(secretBundle, subscriptions));
    }

    private async Task<ErrorOr<Success>> PatchSecretsAsync(IEnumerable<KubernetesBundle> kubernetesBundles, SecretBundle secretBundle, CancellationToken cancellationToken = default)
    {
        List<Task<ErrorOr<Success>>> tasks = [];
        foreach (var kubernetesBundle in kubernetesBundles)
        {
            tasks.Add(secretWriter.PatchAsync(kubernetesBundle.KubernetesSecretName, kubernetesBundle.Namespace, secretBundle, cancellationToken));
        }
        var results = await Task.WhenAll(tasks);
        var withErrors = results.Where(x => x.IsError);
        if(withErrors.Any())
        {
            return withErrors.SelectMany(withErrors => withErrors.Errors).ToList();
        }

        return Result.Success;
    }

    private sealed record Bundle(SecretBundle SecretBundle, IEnumerable<KubernetesBundle> KubernetesBundles);
}
