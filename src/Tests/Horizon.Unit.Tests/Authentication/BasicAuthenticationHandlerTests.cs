using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Horizon.Authentication.Tests;

public class BasicAuthenticationHandlerTests
{
    private readonly Mock<IOptionsMonitor<AuthenticationSchemeOptions>> _optionsMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly AuthenticationConfigProvider _configProvider;
    private readonly Mock<UrlEncoder> _encoderMock;

    public BasicAuthenticationHandlerTests()
    {
        _optionsMock = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        var loggerMock = new Mock<ILogger<BasicAuthenticationHandler>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(loggerMock.Object);
        _optionsMock.Setup(x => x.Get("Basic")).Returns(new AuthenticationSchemeOptions());
        _configProvider = new AuthenticationConfigProvider();
        _encoderMock = new Mock<UrlEncoder>();
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var handler = new BasicAuthenticationHandler(_optionsMock.Object, _loggerFactoryMock.Object, _configProvider, _encoderMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("Authorization", new StringValues("Basic dXNlcm5hbWU6cGFzc3dvcmQ="));
        var expectedUsername = "username";
        var expectedPassword = "password";
        var expectedAuthentication = new BasicAuthentication(expectedUsername, expectedPassword);
        _configProvider.Set(expectedAuthentication);

        // Act
        await handler.InitializeAsync(new AuthenticationScheme("Basic", "Basic", typeof(BasicAuthenticationHandler)), context);
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeTrue();
        handler.Scheme.Name.Should().Be(result?.Ticket?.AuthenticationScheme);
        result?.Principal?.Identity?.Name.Should().Be(expectedUsername);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_InvalidCredentials_ReturnsFailResult()
    {
        // Arrange
        var handler = new BasicAuthenticationHandler(_optionsMock.Object, _loggerFactoryMock.Object, _configProvider, _encoderMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("Authorization", new StringValues("Basic dXNlcm5hbWU6cGFzc3dvcmQ="));
        var expectedUsername = "username";
        var expectedPassword = "wrongpassword";
        var expectedAuthentication = new BasicAuthentication(expectedUsername, expectedPassword);
        _configProvider.Set(expectedAuthentication);

        // Act
        await handler.InitializeAsync(new AuthenticationScheme("Basic", "Basic", typeof(BasicAuthenticationHandler)), context);
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure?.Message.Should().Be(BasicAuthenticationHandler.InvalidUsernameOrPasswordMessage);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_MissingAuthorizationHeader_ReturnsFailResult()
    {
        // Arrange
        var handler = new BasicAuthenticationHandler(_optionsMock.Object, _loggerFactoryMock.Object, _configProvider, _encoderMock.Object);
        var context = new DefaultHttpContext();

        // Act
        await handler.InitializeAsync(new AuthenticationScheme("Basic", "Basic", typeof(BasicAuthenticationHandler)), context);
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure?.Message.Should().Be(BasicAuthenticationHandler.MissingAuthorizationHeaderMessage);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_InvalidAuthorizationHeader_ReturnsFailResult()
    {
        // Arrange
        var handler = new BasicAuthenticationHandler(_optionsMock.Object, _loggerFactoryMock.Object, _configProvider, _encoderMock.Object);
        var context = new DefaultHttpContext();
        context.Request.Headers.Append("Authorization", new StringValues("Invalid"));

        // Act
        await handler.InitializeAsync(new AuthenticationScheme("Basic", "Basic", typeof(BasicAuthenticationHandler)), context);
        var result = await handler.AuthenticateAsync();

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Failure?.Message.Should().Be(BasicAuthenticationHandler.InvalidAuthorizationHeaderMessage);
    }
}
