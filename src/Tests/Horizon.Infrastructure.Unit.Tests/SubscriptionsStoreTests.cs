using ErrorOr;
using FluentAssertions;
using Horizon.Application;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Horizon.Infrastructure.Unit.Tests;

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

    [Fact]
    public void RemoveSubscription_ShouldRemoveBundle_WhenBundleExists()
    {
        // Arrange
        var store = new SubscriptionsStore();
        var azureKeyVaultName = "TestKeyVault";
        var bundleToRemove = new KubernetesBundle("secretName", "prefix", "namespace");

        store.AddSubscription(azureKeyVaultName, bundleToRemove);

        // Act
        var result = store.RemoveSubscription(azureKeyVaultName, bundleToRemove);

        // Assert
        result.IsError.Should().BeFalse();
        var getSubscriptionResult = store.GetSubscription(azureKeyVaultName);
        getSubscriptionResult.IsError.Should().BeFalse();
        getSubscriptionResult.Value.Should().BeEmpty();
    }

    [Fact]
    public void RemoveSubscription_ShouldReturnNotFound_WhenBundleDoesNotExist()
    {
        // Arrange
        var store = new SubscriptionsStore();
        var azureKeyVaultName = "TestKeyVault";
        var bundleToRemove = new KubernetesBundle("secretName", "prefix", "namespace");

        // Act
        var result = store.RemoveSubscription(azureKeyVaultName, bundleToRemove);

        // Assert
        result.IsError.Should().BeTrue();
    }
}
