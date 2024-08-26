using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Horizon.Authentication.Tests;

public class AzureAdAuthenticationHandlerTests
{
    private readonly Mock<IOptionsMonitor<OpenIdConnectOptions>> _optionsMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<AuthenticationConfigProvider> _configProviderMock;
    private readonly Mock<UrlEncoder> _encoderMock;

    public AzureAdAuthenticationHandlerTests()
    {
        _optionsMock = new Mock<IOptionsMonitor<OpenIdConnectOptions>>();
        _optionsMock.Setup(x => x.Get("AzureAd")).Returns(new OpenIdConnectOptions());
        var loggerMock = new Mock<ILogger<AzureAdAuthenticationHandler>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(loggerMock.Object);
        _configProviderMock = new Mock<AuthenticationConfigProvider>();
        _encoderMock = new Mock<UrlEncoder>();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithMissingAuthorizationHeader_ReturnsFailResult()
    {
        // Arrange
        var handler = CreateAzureAdAuthenticationHandler();
        var context = new DefaultHttpContext();
        await handler.InitializeAsync(new AuthenticationScheme("AzureAd", "AzureAd", typeof(AzureAdAuthenticationHandler)), context);
        
        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure?.Message.Should().Be(AzureAdAuthenticationHandler.MissingAuthorizationHeaderMessage);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithInvalidAuthorizationHeader_ReturnsFailResult()
    {
        // Arrange
        var handler = CreateAzureAdAuthenticationHandler();
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("Authorization", new StringValues("Invalid"));
        _configProviderMock.Setup(x => x.Get<AzureAdAuthentication>()).Returns(new AzureAdAuthentication("tenantId", "clientId"));

        // Act
        await handler.InitializeAsync(new AuthenticationScheme("AzureAd", "AzureAd", typeof(AzureAdAuthenticationHandler)), context);
        var result = await handler.AuthenticateAsync();

        result.Succeeded.Should().BeFalse();
        result.Failure?.Message.Should().Be(AzureAdAuthenticationHandler.InvalidTokenMessage);
    }

    private AzureAdAuthenticationHandler CreateAzureAdAuthenticationHandler()
    {
        return new AzureAdAuthenticationHandler(
            _optionsMock.Object,
            _loggerFactoryMock.Object,
            _configProviderMock.Object,
            _encoderMock.Object);
    }
}
