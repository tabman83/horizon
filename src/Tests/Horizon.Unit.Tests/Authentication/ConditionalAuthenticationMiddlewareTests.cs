using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Horizon.Authentication.Unit.Tests;

public class ConditionalAuthenticationMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoAuthentication_ShouldCallNext()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var configProvider = new AuthenticationConfigProvider();
        configProvider.Set(new NoAuthentication());
        var context = new DefaultHttpContext();
        var middleware = new ConditionalAuthenticationMiddleware(nextMock.Object, configProvider);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_AzureAdAuthenticationSucceeded_ShouldCallNext()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var configProvider = new AuthenticationConfigProvider();
        configProvider.Set(new AzureAdAuthentication("tenantId", "clientId"));
        var context = new DefaultHttpContext();
        var middleware = new ConditionalAuthenticationMiddleware(nextMock.Object, configProvider);

        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock.Setup(x => x.AuthenticateAsync(context, "AzureAD")).ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(), "AzureAD")));
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IAuthenticationService>(authServiceMock.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        context.RequestServices = serviceProvider;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_BasicAuthenticationSucceeded_ShouldCallNext()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var configProvider = new AuthenticationConfigProvider();
        configProvider.Set(new BasicAuthentication("username", "password"));
        var context = new DefaultHttpContext();
        var middleware = new ConditionalAuthenticationMiddleware(nextMock.Object, configProvider);

        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock.Setup(x => x.AuthenticateAsync(context, "Basic")).ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(), "Basic")));
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IAuthenticationService>(authServiceMock.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        context.RequestServices = serviceProvider;

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextMock.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ProbeUrl_ShouldCallNext()
    {
        // Arrange
        var configProvider = new AuthenticationConfigProvider();
        var nextMock = new Mock<RequestDelegate>();
        var httpRequestMock = new Mock<HttpRequest>();
        httpRequestMock.SetupGet(x => x.Path)
            .Returns(new PathString(ConditionalAuthenticationMiddleware.ProbeUrl));
        var contextMock = new Mock<HttpContext>();
        contextMock.SetupGet(x => x.Request)
            .Returns(httpRequestMock.Object);
        var middleware = new ConditionalAuthenticationMiddleware(nextMock.Object, configProvider);

        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock.Setup(x => x.AuthenticateAsync(contextMock.Object, "Basic")).ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(), "Basic")));
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IAuthenticationService>(authServiceMock.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Act
        await middleware.InvokeAsync(contextMock.Object);

        // Assert
        nextMock.Verify(next => next(contextMock.Object), Times.Once);
    }
}
