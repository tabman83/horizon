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

public class AzureKeyVaultSubscriptionAddedHandlerTests
{
    private readonly Mock<IKeyVaultSecretReader> _secretReaderMock;
    private readonly Mock<ISubscriptionsStore> _storeMock;
    private readonly Mock<IKubernetesSecretWriter> _secretWriterMock;
    private readonly AzureKeyVaultSubscriptionAddedHandler _handler;

    public AzureKeyVaultSubscriptionAddedHandlerTests()
    {
        _secretReaderMock = new Mock<IKeyVaultSecretReader>();
        _storeMock = new Mock<ISubscriptionsStore>();
        _secretWriterMock = new Mock<IKubernetesSecretWriter>();
        _handler = new AzureKeyVaultSubscriptionAddedHandler(
            _secretReaderMock.Object,
            _storeMock.Object,
            _secretWriterMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleAzureKeyVaultSubscriptionAddedRequest()
    {
        // Arrange

        _storeMock
            .Setup(x => x.AddSubscription("AzureKeyVault1", new KubernetesBundle("K8sSecretObject1", "SecretPrefix1", "Namespace1")))
            .Returns(Result.Success)
            .Verifiable();
        _storeMock
            .Setup(x => x.AddSubscription("AzureKeyVault2", new KubernetesBundle("K8sSecretObject2", "SecretPrefix2", "Namespace1")))
            .Returns(Result.Success)
            .Verifiable();

        var secretList1 = new List<SecretBundle>([new SecretBundle("SecretName1", "SecretValue1")]);
        var secretList2 = new List<SecretBundle>([new SecretBundle("SecretName2", "SecretValue2")]);

        _secretReaderMock.Setup(x => x.LoadAllSecretsAsync("AzureKeyVault1", "SecretPrefix1", default))
            .ReturnsAsync(secretList1)
            .Verifiable();

        _secretReaderMock.Setup(x => x.LoadAllSecretsAsync("AzureKeyVault2", "SecretPrefix2", default))
            .ReturnsAsync(secretList2)
            .Verifiable();

        _secretWriterMock.Setup(x => x.ReplaceAsync("K8sSecretObject1", "Namespace1", secretList1, default))
            .ReturnsAsync(Result.Success)
            .Verifiable();

        _secretWriterMock.Setup(x => x.ReplaceAsync("K8sSecretObject2", "Namespace1", secretList2, default))
            .ReturnsAsync(Result.Success)
            .Verifiable();

        var mappings = new List<AzureKeyVaultMapping>
        {
            new ("AzureKeyVault1", "K8sSecretObject1", "SecretPrefix1"),
            new ("AzureKeyVault2", "K8sSecretObject2", "SecretPrefix2")
        };
        var request = new AzureKeyVaultSubscriptionAddedRequest(mappings, "Namespace1");

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsError.Should().BeFalse();
        _storeMock.Verify();
        _secretReaderMock.Verify();
        _secretWriterMock.Verify();
    }
}
