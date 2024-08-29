using FluentAssertions;
using Horizon.Infrastructure.Kubernetes.Models;
using Horizon.Reconciliators;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Horizon.Unit.Tests.Reconciliators;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddReconciliators_ShouldRegisterReconciliators()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddReconciliators();

        // Assert
        services.Should().HaveCount(2);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IReconciliator<AzureKeyVaultSubscriptionObject>) &&
            descriptor.ImplementationType == typeof(SubscriptionReconciliator) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);
        services.Should().ContainSingle(descriptor =>
            descriptor.ServiceType == typeof(IReconciliator<HorizonProviderConfigurationObject>) &&
            descriptor.ImplementationType == typeof(ConfigurationReconciliator) &&
            descriptor.Lifetime == ServiceLifetime.Singleton);

    }
}
