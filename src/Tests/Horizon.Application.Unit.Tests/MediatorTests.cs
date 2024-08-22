using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Horizon.Application.Tests;

public class MediatorTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mediator _mediator;

    public MediatorTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _mediator = new Mediator(_serviceProviderMock.Object);
    }

    [Fact]
    public async Task SendAsync_WithValidRequest_ReturnsResponse()
    {
        // Arrange
        var request = new TestRequest();
        var response = new TestResponse();
        var handlerMock = new Mock<IAsyncRequestHandler<TestRequest, TestResponse>>();
        handlerMock.Setup(h => h.HandleAsync(request, CancellationToken.None)).ReturnsAsync(response);
        _serviceProviderMock.Setup(s => s.GetService(typeof(IAsyncRequestHandler<TestRequest, TestResponse>))).Returns(handlerMock.Object);

        // Act
        var result = await _mediator.SendAsync<TestRequest, TestResponse>(request);

        // Assert
        Assert.Equal(response, result);
    }

    [Fact]
    public async Task SendAsync_WithInvalidRequest_ThrowsException()
    {
        // Arrange
        var request = new TestRequest();
        var handlerMock = new Mock<IAsyncRequestHandler<TestRequest, TestResponse>>();
        handlerMock.Setup(h => h.HandleAsync(request, CancellationToken.None)).ThrowsAsync(new Exception());
        _serviceProviderMock.Setup(s => s.GetService(typeof(IAsyncRequestHandler<TestRequest, TestResponse>))).Returns(handlerMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(() => _mediator.SendAsync<TestRequest, TestResponse>(request));
    }

    // Test classes for demonstration purposes
    public class TestRequest : IRequest<TestResponse>
    {
    }

    public class TestResponse
    {
    }
}
