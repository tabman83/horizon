using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.Kubernetes;
using Horizon.Infrastructure.Kubernetes.Models;
using Horizon.Reconciliators;
using Microsoft.Extensions.Hosting;

namespace Horizon;

public sealed class HostedService(
    IKubernetesWatcher watcher,
    IReconciliator<HorizonProviderConfigurationObject> configurationReconciliator,
    IReconciliator<AzureKeyVaultSubscriptionObject> subscriptionReconciliator) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            var configTask = watcher.WatchAsync<HorizonProviderConfigurationObject, HorizonProviderConfigurationSpec>(configurationReconciliator.ReconcileAsync, stoppingToken);
            var subscriptionsTask = watcher.WatchAsync<AzureKeyVaultSubscriptionObject, AzureKeyVaultSubscriptionSpec>(subscriptionReconciliator.ReconcileAsync, stoppingToken);
            await Task.WhenAll(configTask, subscriptionsTask);
        }
    }
}
