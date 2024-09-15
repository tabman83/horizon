using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentAssertions;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;
using Horizon.Application.UseCases;
using Moq;
using Xunit;

namespace Horizon.Application.Unit.Tests.UseCases;

public class AzureKeyVaultSubscriptionRemovedHandlerTests
{
    private readonly SubscriptionsStore _store = new ();
    private readonly Mock<IKubernetesSecretWriter> _secretWriterMock = new();
    private readonly AzureKeyVaultSubscriptionRemovedHandler _handler;

    public AzureKeyVaultSubscriptionRemovedHandlerTests()
    {
        _store.AddSubscription("AzureKeyVault1", new KubernetesBundle("K8sSecretObject1", "SecretPrefix1", "Namespace1"));
        _store.AddSubscription("AzureKeyVault2", new KubernetesBundle("K8sSecretObject1", "SecretPrefix2", "Namespace1"));

        _handler = new AzureKeyVaultSubscriptionRemovedHandler(
            _store,
            _secretWriterMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleAzureKeyVaultSubscriptionRemovedRequest()
    {
        // Arrange
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
        _secretWriterMock.Verify();
    }
}
