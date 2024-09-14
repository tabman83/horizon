using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Horizon.Application.Unit.Tests;

public class MediatorTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<ILogger<Mediator>> _loggerMock = new();
    private readonly Mediator _mediator;

    public MediatorTests()
    {
        _mediator = new Mediator(_serviceProviderMock.Object, _loggerMock.Object);
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
    public class TestRequest : IRequest<ErrorOr<TestResponse>>
    {
    }

    public class TestResponse
    {
    }
}
