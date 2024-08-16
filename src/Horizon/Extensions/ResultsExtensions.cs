using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Horizon.Extensions;

public static class ResultsExtensions
{
    public static IResult WithHeaders(this IResult result, IDictionary<string, string> headers)
    {
        return new CustomHeaderResult(result, headers);
    }
}
