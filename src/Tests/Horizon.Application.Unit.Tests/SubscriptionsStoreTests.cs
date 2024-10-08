using ErrorOr;
using FluentAssertions;
using Xunit;

namespace Horizon.Application.Unit.Tests;

public class SubscriptionsStoreTests
{
    private readonly SubscriptionsStore _sut = new ();

    [Fact]
    public void AddSubscription_ShouldAddKubernetesBundleToStore()
    {
        // Arrange
        var azureKeyVaultName = "testKeyVault";
        var kubernetesBundle = new KubernetesBundle("k8sSecretName", null, "namespace");

        // Act
        var result = _sut.AddSubscription(azureKeyVaultName, kubernetesBundle);

        // Assert
        result.IsError.Should().BeFalse();
        _sut.GetSubscription(azureKeyVaultName).Value.Should().OnlyContain(x => x == kubernetesBundle);
    }

    [Fact]
    public void AddSubscription_ShouldAddKubernetesBundlesToStore()
    {
        // Arrange
        var azureKeyVaultName = "testKeyVault";
        var kubernetesBundle1 = new KubernetesBundle("k8sSecretName1", null, "namespace1");
        var kubernetesBundle2 = new KubernetesBundle("k8sSecretName2", null, "namespace2");

        // Act
        _sut.AddSubscription(azureKeyVaultName, kubernetesBundle1);
        var result = _sut.AddSubscription(azureKeyVaultName, kubernetesBundle2);

        // Assert
        // Assert
        result.IsError.Should().BeFalse();
        _sut.GetSubscription(azureKeyVaultName).Value.Should().HaveCount(2);
        _sut.GetSubscription(azureKeyVaultName).Value.Should().OnlyContain(x => x == kubernetesBundle1 || x == kubernetesBundle2);
    }

    [Fact]
    public void AddSubscription_WhenNotFound_ShouldReturnError()
    {
        // Arrange
        var azureKeyVaultName = "testKeyVault";
        var kubernetesBundle1 = new KubernetesBundle("k8sSecretName1", null, "namespace1");
        _sut.AddSubscription(azureKeyVaultName, kubernetesBundle1);

        // Act
        var result = _sut.GetSubscription("notfound");

        // Assert
        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().BeOfType<Error>()
            .Which.Type.Should().Be(ErrorType.NotFound);
    }
}
