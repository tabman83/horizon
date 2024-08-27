using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Horizon.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Horizon.UseCases;

public class WebhookValidationHandler(
    ILogger<WebhookValidationHandler> logger) : IApiHandler
{
    internal const string WebhookRequestOriginHeader = "WebHook-Request-Origin";
    internal const string WebHookAllowedOriginHeader = "WebHook-Allowed-Origin";
    internal const string AllowMethodsHeader = "Allow";
    internal const string WebHookAllowedRateHeader = "WebHook-Allowed-Rate";
    internal const string AllowedMethods = "POST, OPTIONS";
    internal const string WebHookAllowedRate = "*";

    public async Task<IResult> HandleAsync(HttpRequest httpRequest, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        try
        {
            var webhookRequestOrigin = httpRequest.Headers.TryGetValue(WebhookRequestOriginHeader, out var origin) ? origin.ToString() : "Unknown";

            var headers = new Dictionary<string, string>
            {
                [WebHookAllowedOriginHeader] = webhookRequestOrigin,
                [AllowMethodsHeader] = AllowedMethods,
                [WebHookAllowedRateHeader] = WebHookAllowedRate
            };
            return Results.NoContent().WithHeaders(headers);
        }
        catch (TaskCanceledException)
        {
            return Results.Text("Request was canceled", statusCode: 499);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ErrorHandlingWebhookRequest");
            return Results.Text($"Internal Server Error ({exception.Message})", statusCode: 500);
        }
    }
}
