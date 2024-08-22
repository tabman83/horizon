using Azure.Core;
using Azure.Identity;
using Horizon.Application;
using Horizon.Application.AzureKeyVault;
using Horizon.Application.Kubernetes;
using Horizon.Infrastructure.AzureKeyVault;
using Horizon.Infrastructure.Kubernetes;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Horizon.Infrastructure.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddInfrastructureLayer_ShouldRegisterDependencies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInfrastructureLayer();

        // Assert
        Assert.Equal(7, services.Count);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IKubernetes));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IKubernetesWatcher) && descriptor.ImplementationType == typeof(KubernetesWatcher));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IKubernetesSecretWriter) && descriptor.ImplementationType == typeof(KubernetesSecretWriter));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(ISubscriptionsStore) && descriptor.ImplementationType == typeof(SubscriptionsStore));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(SecretClientFactory));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IKeyVaultSecretReader) && descriptor.ImplementationType == typeof(KeyVaultSecretReader));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TokenCredential) && descriptor.ImplementationType == typeof(DefaultAzureCredential));
    }
}
