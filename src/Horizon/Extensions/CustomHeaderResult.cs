using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Horizon.Extensions;

public sealed class CustomHeaderResult(
    IResult innerResult,
    IDictionary<string, string> headers) : IResult
{
    private readonly IResult _innerResult = innerResult;
    private readonly IDictionary<string, string> _headers = headers;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        foreach (var header in _headers)
        {
            httpContext.Response.Headers.Append(header.Key, header.Value);
        }

        await _innerResult.ExecuteAsync(httpContext);
    }
}
