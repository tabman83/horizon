using ErrorOr;
using Horizon.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon.Application.Tests;

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
        Assert.Equal(3, services.Count);
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IMediator) && descriptor.ImplementationType == typeof(Mediator));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAsyncRequestHandler<AzureKeyVaultSubscriptionAddedRequest, ErrorOr<Success>>) && descriptor.ImplementationType == typeof(AzureKeyVaultSubscriptionAddedHandler));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAsyncRequestHandler<AzureKeyVaultSecretNewVersionCreatedRequest, ErrorOr<Success>>) && descriptor.ImplementationType == typeof(AzureKeyVaultSecretNewVersionCreatedHandler));
    }
}
