using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Horizon.Authentication.Tests;

public class ConditionalAuthenticationMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoAuthentication_ShouldCallNext()
    {
        // Arrange
        var nextMock = new Mock<RequestDelegate>();
        var configProviderMock = new Mock<AuthenticationConfigProvider>();
        var context = new DefaultHttpContext();
        var middleware = new ConditionalAuthenticationMiddleware(nextMock.Object, configProviderMock.Object);

        configProviderMock.Setup(c => c.Get<AuthenticationBase>()).Returns(new NoAuthentication());

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
        var configProviderMock = new Mock<AuthenticationConfigProvider>();
        configProviderMock.Setup(x => x.Get<AuthenticationBase>()).Returns(new AzureAdAuthentication("tenantId", "clientId"));
        var context = new DefaultHttpContext();
        var middleware = new ConditionalAuthenticationMiddleware(nextMock.Object, configProviderMock.Object);

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
        var configProviderMock = new Mock<AuthenticationConfigProvider>();
        configProviderMock.Setup(x => x.Get<AuthenticationBase>()).Returns(new BasicAuthentication("username", "password"));
        var context = new DefaultHttpContext();
        var middleware = new ConditionalAuthenticationMiddleware(nextMock.Object, configProviderMock.Object);

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
}
