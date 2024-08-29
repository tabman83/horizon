using FluentAssertions;
using Horizon.Application.Kubernetes;
using Horizon.Authentication;
using Horizon.Infrastructure.Kubernetes.Models;
using Horizon.Reconciliators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Horizon.Unit.Tests.Reconciliators;

public class ConfigurationReconciliatorTests
{
    private readonly FakeLogger<HostedService> _logger = new();
    private readonly AuthenticationConfigProvider _configProvider = new();
    private readonly Mock<IAuthenticationSchemeProvider> _authSchemeProviderMock = new();

    [Fact]
    public async Task ReconcileAsync_WithNullSpec_ShouldLogErrorAndReturnCompletedTask()
    {
        // Arrange
        var reconciliator = new ConfigurationReconciliator(_logger, _configProvider, _authSchemeProviderMock.Object);
        var item = new HorizonProviderConfigurationObject { Spec = (HorizonProviderConfigurationSpec)null! };

        // Act
        await reconciliator.ReconcileAsync(WatchEventType.Added, item);

        // Assert
        _logger.LatestRecord.Message.Should().Be("HorizonProviderConfigurationSpec is null");
    }

    [Fact]
    public async Task ReconcileAsync_WithWebhookAuthenticationNone_ShouldNotSetAuthenticationAndScheme()
    {
        // Arrange
        var reconciliator = new ConfigurationReconciliator(_logger, _configProvider, _authSchemeProviderMock.Object);
        var item = new HorizonProviderConfigurationObject { Spec = new HorizonProviderConfigurationSpec(new AzureKeyVaultAuthentication("None"), new WebhookAuthenticationNone()) };

        // Act
        await reconciliator.ReconcileAsync(WatchEventType.Added, item);

        // Assert
        _authSchemeProviderMock.Verify(x => x.AddScheme(It.IsAny<AuthenticationScheme>()), Times.Never);
    }

    [Fact]
    public async Task ReconcileAsync_WithWebhookAuthenticationBasic_ShouldSetBasicAuthenticationAndScheme()
    {
        // Arrange
        var reconciliator = new ConfigurationReconciliator(_logger, _configProvider, _authSchemeProviderMock.Object);
        var auth = new WebhookAuthenticationBasic("testuser", "testpassword");
        var item = new HorizonProviderConfigurationObject { Spec = new HorizonProviderConfigurationSpec(new AzureKeyVaultAuthentication("None"), auth) };

        // Act
        await reconciliator.ReconcileAsync(WatchEventType.Added, item);

        // Assert
        _authSchemeProviderMock.Verify(x => x.AddScheme(It.Is<AuthenticationScheme>(y => y.HandlerType == typeof(BasicAuthenticationHandler))), Times.Once);
    }

    [Fact]
    public async Task ReconcileAsync_WithWebhookAuthenticationAzureAD_ShouldSetAzureAdAuthenticationAndScheme()
    {
        // Arrange
        var reconciliator = new ConfigurationReconciliator(_logger, _configProvider, _authSchemeProviderMock.Object);
        var auth = new WebhookAuthenticationAzureAD("testtenantid", "testclientid");
        var item = new HorizonProviderConfigurationObject { Spec = new HorizonProviderConfigurationSpec(new AzureKeyVaultAuthentication("None"), auth) };

        // Act
        await reconciliator.ReconcileAsync(WatchEventType.Added, item);

        // Assert
        _authSchemeProviderMock.Verify(x => x.AddScheme(It.Is<AuthenticationScheme>(y => y.HandlerType == typeof(AzureAdAuthenticationHandler))), Times.Once);
    }

    [Fact]
    public async Task ReconcileAsync_WithNoWebhookAuthentication_ShouldLogError()
    {
        // Arrange
        var reconciliator = new ConfigurationReconciliator(_logger, _configProvider, _authSchemeProviderMock.Object);
        var auth = new WebhookAuthenticationNone();
        var item = new HorizonProviderConfigurationObject { Spec = new HorizonProviderConfigurationSpec(new AzureKeyVaultAuthentication("None"), auth) };

        // Act
        await reconciliator.ReconcileAsync(WatchEventType.Added, item);

        // Assert
        _authSchemeProviderMock.Verify(x => x.AddScheme(It.IsAny<AuthenticationScheme>()), Times.Never);
    }
}
