using System.Collections.Generic;
using System.Linq;
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

public class AzureKeyVaultSubscriptionAddedHandlerTests
{
    private readonly Mock<IKeyVaultSecretReader> _secretReaderMock = new();
    private readonly SubscriptionsStore _store = new();
    private readonly Mock<IKubernetesSecretWriter> _secretWriterMock = new();
    private readonly AzureKeyVaultSubscriptionAddedHandler _handler;

    public AzureKeyVaultSubscriptionAddedHandlerTests()
    {
        _handler = new AzureKeyVaultSubscriptionAddedHandler(
            _secretReaderMock.Object,
            _store,
            _secretWriterMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleAzureKeyVaultSubscriptionAddedRequest()
    {
        // Arrange
        var secretList1 = new List<SecretBundle>([new SecretBundle("SecretName1", "SecretValue1")]);
        var secretList2 = new List<SecretBundle>([new SecretBundle("SecretName2", "SecretValue2")]);

        _secretReaderMock.Setup(x => x.LoadAllSecretsAsync("AzureKeyVault1", "SecretPrefix1", default))
            .ReturnsAsync(secretList1)
            .Verifiable();

        _secretReaderMock.Setup(x => x.LoadAllSecretsAsync("AzureKeyVault2", "SecretPrefix2", default))
            .ReturnsAsync(secretList2)
            .Verifiable();

        _secretWriterMock.Setup(x => x.ReplaceAsync("K8sSecretObject1", "Namespace1", secretList1.Concat(secretList2), default))
            .ReturnsAsync(Result.Success)
            .Verifiable();

        var mappings = new List<AzureKeyVaultMapping>
        {
            new ("AzureKeyVault1", "SecretPrefix1"),
            new ("AzureKeyVault2", "SecretPrefix2")
        };
        var request = new AzureKeyVaultSubscriptionAddedRequest(mappings, "K8sSecretObject1", "Namespace1");

        // Act
        var result = await _handler.HandleAsync(request, default);

        // Assert
        result.IsError.Should().BeFalse();
        _secretReaderMock.Verify();
        _secretWriterMock.Verify();
    }
}
