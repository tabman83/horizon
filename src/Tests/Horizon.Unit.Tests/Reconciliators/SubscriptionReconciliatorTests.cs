using System;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using FluentAssertions;
using Horizon.Application;
using Horizon.Application.Kubernetes;
using Horizon.Application.UseCases;
using Horizon.Infrastructure.Kubernetes.Models;
using Horizon.Reconciliators;
using k8s.Models;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

namespace Horizon.Unit.Tests.Reconciliators;

public class SubscriptionReconciliatorTests
{
    private readonly FakeLogger<HostedService> _logger = new();
    private readonly Mock<IMediator> _mediatorMock = new();

    [Theory]
    [InlineData(WatchEventType.Added)]
    [InlineData(WatchEventType.Modified)]
    public async Task ReconcileAsync_ShouldCallHandleVaultsAddedAsync(WatchEventType type)
    {
        // Arrange
        var reconciliator = new SubscriptionReconciliator(_logger, _mediatorMock.Object);
        var item = new AzureKeyVaultSubscriptionObject
        {
            Metadata = new V1ObjectMeta
            {
                Name = "TestSubscription",
                NamespaceProperty = "TestNamespace"
            },
            Spec = new AzureKeyVaultSubscriptionSpec("TestSecret", [new AzureKeyVaultSubscription("TestVault", "TestPrefix")])
        };

        // Act
        await reconciliator.ReconcileAsync(type, item);

        // Assert
        _mediatorMock.Verify(m => m.SendAsync<AzureKeyVaultSubscriptionAddedRequest, Success>(
            It.Is<AzureKeyVaultSubscriptionAddedRequest>(r =>
                r.K8sSecretObjectName == "TestSecret" &&
                r.AzureKeyVaults.Count() == 1 &&
                r.AzureKeyVaults.First().AzureKeyVaultName == "TestVault" &&
                r.AzureKeyVaults.First().SecretPrefix == "TestPrefix" &&
                r.Namespace == "TestNamespace"
            ), default
        ), Times.Once);
        _logger.LatestRecord.Message.Should().Be("AzureKeyVaultSubscriptionAdded");
    }

    [Theory]
    [InlineData(WatchEventType.Deleted)]
    public async Task ReconcileAsync_WithDeletedEventType_ShouldCallHandleVaultsRemovedAsync(WatchEventType type)
    {
        // Arrange
        var reconciliator = new SubscriptionReconciliator(_logger, _mediatorMock.Object);
        var item = new AzureKeyVaultSubscriptionObject
        {
            Metadata = new V1ObjectMeta
            {
                Name = "TestSubscription",
                NamespaceProperty = "TestNamespace"
            },
            Spec = new AzureKeyVaultSubscriptionSpec("TestSecret", [new AzureKeyVaultSubscription("TestVault", "TestPrefix")])
        };

        // Act
        await reconciliator.ReconcileAsync(type, item);

        // Assert
        _mediatorMock.Verify(m => m.SendAsync<AzureKeyVaultSubscriptionRemovedRequest, Success>(
            It.Is<AzureKeyVaultSubscriptionRemovedRequest>(r =>
                r.K8sSecretObjectName == "TestSecret" &&
                r.AzureKeyVaults.Count() == 1 &&
                r.AzureKeyVaults.First().AzureKeyVaultName == "TestVault" &&
                r.AzureKeyVaults.First().SecretPrefix == "TestPrefix" &&
                r.Namespace == "TestNamespace"
            ), default
        ), Times.Once);
        _logger.LatestRecord.Message.Should().Be("AzureKeyVaultSubscriptionRemoved");
    }

    [Theory]
    [InlineData(WatchEventType.Error)]
    [InlineData(WatchEventType.Bookmark)]
    public async Task ReconcileAsync_WithOtherEventType_ShouldLogEventTypeAndItems(WatchEventType type)
    {
        // Arrange
        var reconciliator = new SubscriptionReconciliator(_logger, _mediatorMock.Object);

        var item = new AzureKeyVaultSubscriptionObject
        {
            Metadata = new V1ObjectMeta
            {
                Name = "TestSubscription",
                NamespaceProperty = "TestNamespace"
            },
            Spec = new AzureKeyVaultSubscriptionSpec("TestSecret", [new AzureKeyVaultSubscription("TestVault", "TestPrefix")])
        };

        // Act
        await reconciliator.ReconcileAsync(type, item);

        // Assert
        _mediatorMock.Verify(m => m.SendAsync<AzureKeyVaultSubscriptionAddedRequest, Success>(It.IsAny<AzureKeyVaultSubscriptionAddedRequest>(), default), Times.Never);
    }
}
