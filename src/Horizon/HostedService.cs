using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using Horizon.Application;
using Horizon.Application.Kubernetes;
using Horizon.Application.UseCases;
using Horizon.Infrastructure.Kubernetes.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizon;

public sealed class HostedService(
    ILogger<HostedService> logger,
    IKubernetesWatcher watcher,
    IMediator mediator) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            var configTask = watcher.WatchAsync<HorizonProviderConfigurationObject, HorizonProviderConfigurationSpec>(ReconcileConfigurationAsync, stoppingToken);
            var subscriptionsTask = watcher.WatchAsync<AzureKeyVaultSubscriptionObject, AzureKeyVaultSubscriptionSpec>(ReconcileSubscriptionsAsync, stoppingToken);
            await Task.WhenAll(configTask, subscriptionsTask);
        }
    }

    private Task ReconcileConfigurationAsync(WatchEventType type, HorizonProviderConfigurationObject item)
    {
        Console.WriteLine("ReconcileConfiguration {0} {1}", type, JsonSerializer.Serialize(item.Spec));
        return Task.CompletedTask;
    }

    private async Task ReconcileSubscriptionsAsync(WatchEventType type, AzureKeyVaultSubscriptionObject @object)
    {
        if (@object.Metadata?.Name is null || @object.Metadata?.NamespaceProperty is null)
        {
            logger.LogError("Name or Namespace is null");
            return;
        }
        if(@object.Spec is null)
        {
            logger.LogError("Spec is null");
            return;
        }
        var vaults = @object.Spec.Vaults;
        switch (type)
        {
            case WatchEventType.Added:
            case WatchEventType.Modified:
                var response = await HandleVaultsAddedAsync(vaults, @object.Metadata.NamespaceProperty);
                response.Switch(
                    _ => logger.LogInformation("AzureKeyVaultSubscriptionAdded"),
                    errors => logger.LogError("AzureKeyVaultSubscriptionAddedErrors {Errors}", errors));
                break;
            case WatchEventType.Deleted:
                Console.WriteLine("Deleted");
                break;
            case WatchEventType.Error:
            case WatchEventType.Bookmark:
                logger.LogInformation("WatchEventType {Type}", type);
                logger.LogInformation("Items {Items}", @object.Spec.Vaults);
                break;
            default:
                logger.LogInformation("Unknown WatchEventType {Type}", type);
                throw new InvalidOperationException($"Unknown WatchEventType {type}");
        };
    }

    private Task<ErrorOr<Success>> HandleVaultsAddedAsync(IEnumerable<AzureKeyVaultSubscription> vaults, string @namespace)
    {
        var mappings = vaults.Select(mapping => new AzureKeyVaultMappingRequest(mapping.AzureKeyVaultName, mapping.K8sSecretObjectName, mapping.SecretPrefix));
        var request = new AzureKeyVaultSubscriptionAddedRequest(mappings, @namespace);
        return mediator.SendAsync<AzureKeyVaultSubscriptionAddedRequest, ErrorOr<Success>>(request);
    }
}
