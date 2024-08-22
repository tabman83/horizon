using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;
using Horizon.Application.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Horizon.Application.Tests.UseCases;

public class AzureKeyVaultSecretNewVersionCreatedHandlerTests
{
    private readonly Mock<ISubscriptionsStore> _storeMock;
    private readonly Mock<IKeyVaultSecretReader> _secretReaderMock;
    private readonly Mock<IKubernetesSecretWriter> _secretWriterMock;

    public AzureKeyVaultSecretNewVersionCreatedHandlerTests()
    {
        _storeMock = new Mock<ISubscriptionsStore>();
        _secretReaderMock = new Mock<IKeyVaultSecretReader>();
        _secretWriterMock = new Mock<IKubernetesSecretWriter>();
    }

    [Fact]
    public async Task HandleAsync_ShouldPatchSecretsForAllKubernetesBundles_WhenNewVersionCreated()
    {
        // Arrange
        var request = new AzureKeyVaultSecretNewVersionCreatedRequest("vaultName", "secretName");
        var secretBundle = new SecretBundle("secretName", "secretValue");
        var kubernetesBundles = new List<KubernetesBundle>
        {
            new("secret1", "namespace1"),
            new("secret2", "namespace2")
        };

        _secretReaderMock.Setup(x => x.LoadSingleSecretAsync(request.VaultName, request.SecretName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(secretBundle);
        _storeMock.Setup(x => x.GetSubscription(request.VaultName))
            .Returns(kubernetesBundles);
        _secretWriterMock.Setup(x => x.PatchAsync(It.IsAny<string>(), It.IsAny<string>(), secretBundle, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var handler = new AzureKeyVaultSecretNewVersionCreatedHandler(
            _storeMock.Object,
            _secretReaderMock.Object,
            _secretWriterMock.Object);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsError);

        foreach (var kubernetesBundle in kubernetesBundles)
        {
            _secretWriterMock.Verify(x => x.PatchAsync(kubernetesBundle.KubernetesSecretName, kubernetesBundle.Namespace, secretBundle, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
