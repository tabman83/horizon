using System.Threading;
using System.Threading.Tasks;
using Horizon.Application.Kubernetes;
using Horizon.Infrastructure.Kubernetes.Models;
using Horizon.Reconciliators;
using Moq;
using Xunit;

namespace Horizon.Unit.Tests;

public class HostedServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldWatchConfigAndSubscriptions()
    {
        // Arrange
        var watcherMock = new Mock<IKubernetesWatcher>();
        var configurationReconciliatorMock = new Mock<IReconciliator<HorizonProviderConfigurationObject>>();
        var subscriptionReconciliatorMock = new Mock<IReconciliator<AzureKeyVaultSubscriptionObject>>();

        var hostedService = new HostedService(
            watcherMock.Object,
            configurationReconciliatorMock.Object,
            subscriptionReconciliatorMock.Object
        );

        watcherMock.Setup(w => w.WatchAsync<HorizonProviderConfigurationObject, HorizonProviderConfigurationSpec>(
            It.IsAny<ReconcileDelegate<HorizonProviderConfigurationObject>>(), It.IsAny<CancellationToken>()))
            .Returns(GetCancellableTask<ReconcileDelegate<HorizonProviderConfigurationObject>>)
            .Verifiable();

        watcherMock.Setup(w => w.WatchAsync<AzureKeyVaultSubscriptionObject, AzureKeyVaultSubscriptionSpec>(
            It.IsAny<ReconcileDelegate<AzureKeyVaultSubscriptionObject>>(), It.IsAny<CancellationToken>()))
            .Returns(GetCancellableTask<ReconcileDelegate<AzureKeyVaultSubscriptionObject>>)
            .Verifiable();

        // Act
        await hostedService.StartAsync(default);
        await Task.Delay(200);
        await hostedService.StopAsync(default);

        // Assert
        watcherMock.Verify();
        watcherMock.Verify();
    }

    private static Task GetCancellableTask<T>(T _, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource();
        ct.Register(() => tcs.TrySetCanceled());
        return tcs.Task;
    }
}
