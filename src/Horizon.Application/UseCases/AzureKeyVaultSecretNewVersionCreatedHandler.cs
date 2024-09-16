using ErrorOr;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Horizon.Application.Logging;

namespace Horizon.Application.UseCases;

public sealed record AzureKeyVaultSecretNewVersionCreatedRequest(string VaultName, string SecretName) : IRequest<ErrorOr<Success>>;

public class AzureKeyVaultSecretNewVersionCreatedHandler(
    SubscriptionsStore store,
    ILogger<AzureKeyVaultSecretNewVersionCreatedHandler> logger,
    IKeyVaultSecretReader secretReader,
    IKubernetesSecretWriter secretWriter) : IAsyncRequestHandler<AzureKeyVaultSecretNewVersionCreatedRequest, Success>
{
    public async Task<ErrorOr<Success>> HandleAsync(AzureKeyVaultSecretNewVersionCreatedRequest request, CancellationToken cancellationToken = default)
    {
        using (logger
            .With("KeyVaultName", request.VaultName)
            .With("SecretName", request.SecretName).BeginScope())
        {
            return await secretReader.LoadSingleSecretAsync(request.VaultName, request.SecretName, cancellationToken)
                .Then(secret => GetSubscription(secret, request.VaultName))
                .MatchFirstAsync(bundle => PatchSecretsAsync(bundle, cancellationToken), SkipNotFoundAsync);
        }
    }

    private async Task<ErrorOr<Success>> SkipNotFoundAsync(Error error)
    {
        await Task.Yield();
        if (error.Type is ErrorType.NotFound)
        {
            logger.LogInformation("NoAzureKeyVaultSubscriptionConfigForKeyVault");
            return Result.Success;
        }
        return error;
    }

    private ErrorOr<Bundle> GetSubscription(SecretBundle secretBundle, string vaultName)
    {
        return store.GetSubscription(vaultName)
            .Then(subscriptions => new Bundle(secretBundle, subscriptions));
    }

    private async Task<ErrorOr<Success>> PatchSecretsAsync(Bundle bundle, CancellationToken cancellationToken = default)
    {
        List<Task<ErrorOr<Success>>> tasks = [];
        foreach (var kubernetesBundle in bundle.KubernetesBundles)
        {
            var task = secretWriter.PatchAsync(
                kubernetesBundle.KubernetesSecretName,
                kubernetesBundle.Namespace,
                bundle.SecretBundle,
                cancellationToken);
            tasks.Add(task);
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