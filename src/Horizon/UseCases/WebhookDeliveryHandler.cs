using System;
using ErrorOr;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Horizon.Application;
using Horizon.Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure;

namespace Horizon.UseCases;

public class WebhookDeliveryHandler(
    ILogger<WebhookDeliveryHandler> logger,
    IMediator mediator) : IApiHandler
{
    public async Task<IResult> HandleAsync(HttpRequest httpRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var binaryData = await BinaryData.FromStreamAsync(httpRequest.Body, cancellationToken);
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
                    var request = new AzureKeyVaultSecretNewVersionCreatedRequest(data.VaultName, data.ObjectName);
                    var response = await mediator.SendAsync<AzureKeyVaultSecretNewVersionCreatedRequest, ErrorOr<Success>>(request, cancellationToken);
                    response.Switch(
                        success => logger.LogInformation("KeyVaultSecretNewVersionCreatedSuccess"),
                        errors => logger.LogError("KeyVaultSecretNewVersionCreatedError {Errors}", errors));
                    return Results.Ok();
                default:
                    logger.LogInformation("UnhandledEventType {EventType} {Event}", @event.Type, @event.Data?.ToString());
                    return Results.Ok();
            }
        }
        catch (TaskCanceledException)
        {
            return Results.Text("RequestWasCanceled", statusCode: 499);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ErrorHandlingWebhookDelivery");
            return Results.Ok();
        }
    }
}
