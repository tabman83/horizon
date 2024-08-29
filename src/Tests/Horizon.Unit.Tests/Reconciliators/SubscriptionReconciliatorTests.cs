using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
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

    [Theory, AutoData]
    [InlineAutoData(WatchEventType.Added)]
    [InlineAutoData(WatchEventType.Modified)]
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
            Spec = new AzureKeyVaultSubscriptionSpec([new AzureKeyVaultSubscription("TestVault", "TestSecret", "TestPrefix")])
        };

        // Act
        await reconciliator.ReconcileAsync(type, item);

        // Assert
        _mediatorMock.Verify(m => m.SendAsync<AzureKeyVaultSubscriptionAddedRequest, ErrorOr<Success>>(
            It.Is<AzureKeyVaultSubscriptionAddedRequest>(r =>
                r.Mappings.Count() == 1 &&
                r.Mappings.First().AzureKeyVaultName == "TestVault" &&
                r.Mappings.First().K8sSecretObjectName == "TestSecret" &&
                r.Mappings.First().SecretPrefix == "TestPrefix" &&
                r.Namespace == "TestNamespace"
            ), default
        ), Times.Once);
        _logger.LatestRecord.Message.Should().Be("AzureKeyVaultSubscriptionAdded");
    }

    [Fact]
    public async Task ReconcileAsync_WithDeletedEventType_ShouldNotCallHandleVaultsAddedAsync()
    {
        // Arrange
        var reconciliator = new SubscriptionReconciliator(_logger, _mediatorMock.Object);

        var type = WatchEventType.Deleted;
        var item = new AzureKeyVaultSubscriptionObject();

        // Act
        await reconciliator.ReconcileAsync(type, item);

        // Assert
        _mediatorMock.Verify(m => m.SendAsync<AzureKeyVaultSubscriptionAddedRequest, ErrorOr<Success>>(It.IsAny<AzureKeyVaultSubscriptionAddedRequest>(), default), Times.Never);
    }

    [Theory, AutoData]
    [InlineAutoData(WatchEventType.Error)]
    [InlineAutoData(WatchEventType.Bookmark)]
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
            Spec = new AzureKeyVaultSubscriptionSpec([new AzureKeyVaultSubscription("TestVault", "TestSecret", "TestPrefix")])
        };

        // Act
        await reconciliator.ReconcileAsync(type, item);

        // Assert
        _mediatorMock.Verify(m => m.SendAsync<AzureKeyVaultSubscriptionAddedRequest, ErrorOr<Success>>(It.IsAny<AzureKeyVaultSubscriptionAddedRequest>(), default), Times.Never);
    }
}
