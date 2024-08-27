using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Horizon.Extensions.Unit.Tests;

public class CustomHeaderResultTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldSetHeadersAndExecuteInnerResult()
    {
        // Arrange
        var innerResultMock = new Mock<IResult>();
        var headers = new Dictionary<string, string>
        {
            { "Header1", "Value1" },
            { "Header2", "Value2" }
        };
        var customHeaderResult = new CustomHeaderResult(innerResultMock.Object, headers);
        var httpContext = new DefaultHttpContext();
        var responseMock = new Mock<HttpResponse>();
        var headersDictionary = new HeaderDictionary();

        responseMock.SetupGet(r => r.Headers).Returns(headersDictionary);

        // Act
        await customHeaderResult.ExecuteAsync(httpContext);

        //Assert
        httpContext.Response.Headers.Should().HaveCount(headers.Count);
        foreach (var header in httpContext.Response.Headers)
        {
            httpContext.Response.Headers[header.Key].Should().BeEquivalentTo(headers[header.Key]);
        }
        innerResultMock.Verify(r => r.ExecuteAsync(httpContext), Times.Once);
    }
}
