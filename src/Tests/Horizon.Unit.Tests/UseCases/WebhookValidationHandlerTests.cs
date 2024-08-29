using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Horizon.Extensions;
using Horizon.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Horizon.Unit.Tests.UseCases;

public class WebhookValidationHandlerTests
{
    private readonly Mock<ILogger<WebhookValidationHandler>> _loggerMock;
    private readonly WebhookValidationHandler _sut;

    public WebhookValidationHandlerTests()
    {
        _loggerMock = new Mock<ILogger<WebhookValidationHandler>>();
        _sut = new WebhookValidationHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnOkResultWithHeaders()
    {
        // Arrange
        var httpRequestMock = new Mock<HttpRequest>();
        var headers = new HeaderDictionary
        {
            { WebhookValidationHandler.WebhookRequestOriginHeader, "http://example.com" }
        };
        httpRequestMock.SetupGet(x => x.Headers).Returns(headers);

        // Act
        var result = await _sut.HandleAsync(httpRequestMock.Object, default);

        // Assert
        result.Should().BeOfType<CustomHeaderResult>();
    }

    [Fact]
    public async Task HandleAsync_WhenThrowsException_ShouldReturnError()
    {
        // Arrange
        var httpRequestMock = new Mock<HttpRequest>();
        httpRequestMock.SetupGet(x => x.Headers)
            .Throws(new InvalidOperationException());

        // Act
        var result = await _sut.HandleAsync(httpRequestMock.Object, default);

        // Assert
        result.Should().BeOfType<ContentHttpResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
