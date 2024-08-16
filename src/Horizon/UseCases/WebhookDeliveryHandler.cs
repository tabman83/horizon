using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Horizon.UseCases;

public class WebhookDeliveryHandler(
    ILogger<WebhookDeliveryHandler> logger) : IApiHandler
{
    public async Task<IResult> HandleAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var binaryData = await BinaryData.FromStreamAsync(request.Body, cancellationToken);
            var @event = CloudEvent.Parse(binaryData);

            if(@event is null)
            {
                logger.LogInformation("EventIsNull");
                return Results.Ok();
            }
            if (!@event.TryGetSystemEventData(out object eventData))
            {
                logger.LogInformation("EventTypeNotAvailable {EventType} {Event}", @event.Type, @event.Data?.ToString());
                return Results.Ok();
            }
            switch (eventData)
            {
                case KeyVaultSecretNewVersionCreatedEventData data:
                    logger.LogInformation("KeyVaultSecretNewVersionCreated {Data}", data);
                    return Results.Ok();
                default:
                    logger.LogInformation("UnhandledEventType {EventType} {Event}", @event.Type, @event.Data?.ToString());
                    return Results.Ok();
            }
        }
        catch (TaskCanceledException)
        {
            return Results.Text("Request was canceled", statusCode: 499);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ErrorHandlingWebhookDelivery");
            return Results.Text($"Internal Server Error ({exception.Message})", statusCode: 500);
        }
    }
}
