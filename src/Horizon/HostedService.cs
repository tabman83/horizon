using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application;
using Horizon.Application.UseCases;
using Horizon.Models;
using k8s;
using k8s.Autorest;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizon;

public sealed class HostedService(
    ILogger<HostedService> logger,
    IKubernetes client,
    IMediator mediator) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var watch = await GetAzureKeyVaultSubscriptionWatcherAsync(stoppingToken);

        using (watch.Watch<AzureKeyVaultSubscriptionObject, AzureKeyVaultSubscriptionObject>(Reconcile))
        {
            logger.LogInformation("WatchingCustomObject");
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }

    private Task<HttpOperationResponse<AzureKeyVaultSubscriptionObject>> GetAzureKeyVaultSubscriptionWatcherAsync(CancellationToken cancellationToken = default)
    {
        return client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync<AzureKeyVaultSubscriptionObject>(
            group: AzureKeyVaultSubscriptionObject.Group,
            version: AzureKeyVaultSubscriptionObject.Version,
            plural: AzureKeyVaultSubscriptionObject.Plural,
            watch: true,
            cancellationToken: cancellationToken);
    }

    private void Reconcile(WatchEventType type, AzureKeyVaultSubscriptionObject item)
    {
        switch(type)
        {
            case WatchEventType.Added:
                var mappings = item.Spec.Select(mapping => new AzureKeyVaultMappingRequest(mapping.AzureKeyVaultUrl, mapping.K8sSecretObjectName));
                var request = new AzureKeyVaultSubscriptionAddedRequest(mappings);
                _ = mediator.SendAsync<AzureKeyVaultSubscriptionAddedRequest, AzureKeyVaultSubscriptionAddedResponse>(request);
                Console.WriteLine("Added");
                break;
            case WatchEventType.Deleted:
                Console.WriteLine("Deleted");
                break;
            case WatchEventType.Modified:
                Console.WriteLine("Modified");
                break;
            case WatchEventType.Error:
            case WatchEventType.Bookmark:
                logger.LogInformation("WatchEventType {Type}", type);
                logger.LogInformation("Item {Item}", item);
                break;
            default:
                logger.LogInformation("Unknown WatchEventType {Type}", type);
                throw new InvalidOperationException($"Unknown WatchEventType {type}");
        };
    }
}
