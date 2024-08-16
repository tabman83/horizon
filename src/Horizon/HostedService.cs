using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Application;
using Horizon.Application.Kubernetes;
using Horizon.Application.UseCases;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizon;

public sealed class HostedService(
    ILogger<HostedService> logger,
    IKubernetesWatcher watcher,
    IMediator mediator) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        watcher.RunWatcherAsync(Reconcile, stoppingToken);

    private void Reconcile(WatchEventType type, string? name, string? @namespace, IEnumerable<AzureKeyVaultSubscriptionSpec> items)
    {
        switch(type)
        {
            case WatchEventType.Added:
                var mappings = items.Select(mapping => new AzureKeyVaultMappingRequest(mapping.AzureKeyVaultUrl, mapping.K8sSecretObjectName));
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
                logger.LogInformation("Items {Items}", items);
                break;
            default:
                logger.LogInformation("Unknown WatchEventType {Type}", type);
                throw new InvalidOperationException($"Unknown WatchEventType {type}");
        };
    }
}
