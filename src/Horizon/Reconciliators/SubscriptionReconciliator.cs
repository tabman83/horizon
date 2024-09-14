using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using Horizon.Application;
using Horizon.Application.Kubernetes;
using Horizon.Application.UseCases;
using Horizon.Infrastructure.Kubernetes.Models;
using Microsoft.Extensions.Logging;

namespace Horizon.Reconciliators;

public class SubscriptionReconciliator(
    ILogger<HostedService> logger,
    IMediator mediator) : IReconciliator<AzureKeyVaultSubscriptionObject>
{
    public async Task ReconcileAsync(WatchEventType type, AzureKeyVaultSubscriptionObject item)
    {
        if (item.Metadata?.Name is null || item.Metadata?.NamespaceProperty is null)
        {
            logger.LogError("Name or Namespace is null");
            return;
        }
        if (item.Spec is null)
        {
            logger.LogError("Spec is null");
            return;
        }
        var vaults = item.Spec.Vaults;
        switch (type)
        {
            case WatchEventType.Added:
            case WatchEventType.Modified:
                var addResponse = await HandleVaultsAddedAsync(vaults, item.Metadata.NamespaceProperty);
                addResponse.Switch(
                    _ => logger.LogInformation("AzureKeyVaultSubscriptionAdded"),
                    errors => logger.LogError("AzureKeyVaultSubscriptionAddedErrors {Errors}", errors));
                break;
            case WatchEventType.Deleted:
                var deleteResponse = await HandleVaultsDeletedAsync(vaults, item.Metadata.NamespaceProperty);
                deleteResponse.Switch(
                    _ => logger.LogInformation("AzureKeyVaultSubscriptionRemoved"),
                    errors => logger.LogError("AzureKeyVaultSubscriptionRemovedErrors {Errors}", errors));
                break;
            case WatchEventType.Error:
            case WatchEventType.Bookmark:
                logger.LogInformation("WatchEventType {Type}", type);
                logger.LogInformation("Items {Items}", item.Spec.Vaults);
                break;
            default:
                logger.LogInformation("Unknown WatchEventType {Type}", type);
                throw new InvalidOperationException($"Unknown WatchEventType {type}");
        }
    }

    private Task<ErrorOr<Success>> HandleVaultsAddedAsync(IEnumerable<AzureKeyVaultSubscription> vaults, string @namespace)
    {
        var mappings = vaults.Select(mapping => new AzureKeyVaultMapping(mapping.AzureKeyVaultName, mapping.K8sSecretObjectName, mapping.SecretPrefix));
        var request = new AzureKeyVaultSubscriptionAddedRequest(mappings, @namespace);
        return mediator.SendAsync<AzureKeyVaultSubscriptionAddedRequest, Success>(request);
    }

    private Task<ErrorOr<Success>> HandleVaultsDeletedAsync(IEnumerable<AzureKeyVaultSubscription> vaults, string @namespace)
    {
        var mappings = vaults.Select(mapping => new AzureKeyVaultMapping(mapping.AzureKeyVaultName, mapping.K8sSecretObjectName, mapping.SecretPrefix));
        var request = new AzureKeyVaultSubscriptionRemovedRequest(mappings, @namespace);
        return mediator.SendAsync<AzureKeyVaultSubscriptionRemovedRequest, Success>(request);
    }
}
