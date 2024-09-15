using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentAssertions;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;
using Horizon.Application.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Horizon.Application.Unit.Tests.UseCases;

public class AzureKeyVaultSubscriptionRemovedHandlerTests
{
    private readonly Mock<ISubscriptionsStore> _storeMock;
    private readonly Mock<IKubernetesSecretWriter> _secretWriterMock;
    private readonly AzureKeyVaultSubscriptionRemovedHandler _handler;

    public AzureKeyVaultSubscriptionRemovedHandlerTests()
    {
        _storeMock = new Mock<ISubscriptionsStore>();
        _secretWriterMock = new Mock<IKubernetesSecretWriter>();
        _handler = new AzureKeyVaultSubscriptionRemovedHandler(
            _storeMock.Object,
            _secretWriterMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleAzureKeyVaultSubscriptionRemovedRequest()
    {
        // Arrange

        _storeMock
            .Setup(x => x.RemoveSubscription("AzureKeyVault1", new KubernetesBundle("K8sSecretObject1", "SecretPrefix1", "Namespace1")))
            .Returns(Result.Success)
            .Verifiable();
        _storeMock
            .Setup(x => x.RemoveSubscription("AzureKeyVault2", new KubernetesBundle("K8sSecretObject1", "SecretPrefix2", "Namespace1")))
            .Returns(Result.Success)
            .Verifiable();

        var secretList1 = new List<SecretBundle>([new SecretBundle("SecretName1", "SecretValue1")]);
        var secretList2 = new List<SecretBundle>([new SecretBundle("SecretName2", "SecretValue2")]);

        _secretWriterMock.Setup(x => x.ReplaceAsync("K8sSecretObject1", "Namespace1", new List<SecretBundle>(), default))
            .ReturnsAsync(Result.Success)
            .Verifiable();

        var mappings = new List<AzureKeyVaultMapping>
        {
            new ("AzureKeyVault1", "SecretPrefix1"),
            new ("AzureKeyVault2", "SecretPrefix2")
        };
        var request = new AzureKeyVaultSubscriptionRemovedRequest(mappings, "K8sSecretObject1", "Namespace1");

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsError.Should().BeFalse();
        _storeMock.Verify();
        _secretWriterMock.Verify();
    }
}
