using ErrorOr;
using Horizon.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Horizon.Application.Unit.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplicationLayer_ShouldRegisterDependencies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationLayer();

        // Assert
        Assert.Equal(5, services.Count);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMediator) && descriptor.ImplementationType == typeof(Mediator));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAsyncRequestHandler<AzureKeyVaultSubscriptionRemovedRequest, Success>) && descriptor.ImplementationType == typeof(AzureKeyVaultSubscriptionRemovedHandler));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, Success>) && descriptor.ImplementationType == typeof(AzureKeyVaultSubscriptionAddedHandler));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAsyncRequestHandler<AzureKeyVaultSecretNewVersionCreatedRequest, Success>) && descriptor.ImplementationType == typeof(AzureKeyVaultSecretNewVersionCreatedHandler));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(SubscriptionsStore) && descriptor.ImplementationType == typeof(SubscriptionsStore));
    }
}
