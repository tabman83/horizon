using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging;
using ErrorOr;
using FluentAssertions;
using Horizon.Application;
using Horizon.Application.UseCases;
using Horizon.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Horizon.Unit.Tests.UseCases;

public class WebhookDeliveryHandlerTests
{
    private readonly Mock<ILogger<WebhookDeliveryHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly WebhookDeliveryHandler _webhookDeliveryHandler;

    public WebhookDeliveryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<WebhookDeliveryHandler>>();
        _mediatorMock = new Mock<IMediator>();
        _webhookDeliveryHandler = new WebhookDeliveryHandler(_loggerMock.Object, _mediatorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithNullEvent_ReturnsOkResult()
    {
        // Arrange
        var httpRequest = new Mock<HttpRequest>();
        httpRequest.SetupGet(r => r.Body).Returns(Stream.Null);

        // Act
        var result = await _webhookDeliveryHandler.HandleAsync(httpRequest.Object);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task HandleAsync_WithSystemEventData_ReturnsOkResult()
    {
        // Arrange
        var httpRequest = new Mock<HttpRequest>();
        var @event = GetSampleEvent("sample-kv", "cm--secret-name", "NonSystemEventType");
        httpRequest.SetupGet(r => r.Body).Returns(new MemoryStream(@event));

        // Act
        var result = await _webhookDeliveryHandler.HandleAsync(httpRequest.Object);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task HandleAsync_WithKeyVaultSecretNewVersionCreatedEventData_CallsMediatorAndReturnsOkResult()
    {
        // Arrange
        var httpRequest = new Mock<HttpRequest>();
        var @event = GetSampleEvent("sample-kv", "cm--secret-name", "Microsoft.KeyVault.SecretNewVersionCreated");
        httpRequest.SetupGet(r => r.Body).Returns(new MemoryStream(@event));

        var request = new AzureKeyVaultSecretNewVersionCreatedRequest("sample-kv", "cm--secret-name");
        _mediatorMock.Setup(m => m.SendAsync<AzureKeyVaultSecretNewVersionCreatedRequest, ErrorOr<Success>>(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        // Act
        var result = await _webhookDeliveryHandler.HandleAsync(httpRequest.Object);

        // Assert
        result.Should().BeOfType<NoContent>();
        _mediatorMock.Verify(m => m.SendAsync<AzureKeyVaultSecretNewVersionCreatedRequest, ErrorOr<Success>>(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithUnhandledEventType_ReturnsOkResult()
    {
        // Arrange
        var httpRequest = new Mock<HttpRequest>();
        var @event = GetSampleEvent("sample-kv", "cm--secret-name", "Microsoft.KeyVault.SecretExpired");
        httpRequest.SetupGet(r => r.Body).Returns(new MemoryStream(@event));

        // Act
        var result = await _webhookDeliveryHandler.HandleAsync(httpRequest.Object);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    [Fact]
    public async Task HandleAsync_WithCanceledRequest_ReturnsTextResultWithStatusCode499()
    {
        // Arrange
        var httpRequest = new Mock<HttpRequest>();
        httpRequest.SetupGet(r => r.Body).Throws(new TaskCanceledException());

        // Act
        var result = await _webhookDeliveryHandler.HandleAsync(httpRequest.Object);
        // Assert
        result.Should().BeOfType<ContentHttpResult>()
            .Which.StatusCode.Should().Be(499);
    }

    [Fact]
    public async Task HandleAsync_WithException_ReturnsOkResult()
    {
        // Arrange
        var httpRequest = new Mock<HttpRequest>();
        httpRequest.SetupGet(r => r.Body).Returns(Stream.Null);

        _mediatorMock.Setup(m => m.SendAsync<AzureKeyVaultSecretNewVersionCreatedRequest, ErrorOr<Success>>(It.IsAny<AzureKeyVaultSecretNewVersionCreatedRequest>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception());

        // Act
        var result = await _webhookDeliveryHandler.HandleAsync(httpRequest.Object);

        // Assert
        result.Should().BeOfType<NoContent>();
    }

    private static byte[] GetSampleEvent(string keyVaultName, string secretName, string eventType)
    {
        var eventData = $$"""
        {
            "id":"7f5c52b8-e48b-4f90-b136-92c7f740ef71",
            "source": "/subscriptions/randomguid/resourceGroups/ResourceGroupName/providers/Microsoft.KeyVault/vaults/{{keyVaultName}}",
            "specversion": "1.0",
            "type": "{{eventType}}",
            "subject": "{{secretName}}",
            "time": "2024-08-28T14:04:07.9484189Z",
            "data": {
                "Id": "https://{{keyVaultName}}.vault.azure.net/secrets/{{secretName}}/4b5e61993df54f2ebf7801fb4bc03d9a",
                "VaultName": "{{keyVaultName}}",
                "ObjectType": "Secret",
                "ObjectName": "{{secretName}}",
                "Version": "4b5e61993df54f2ebf7801fb4bc03d9a",
                "NBF": null,
                "EXP": null
            }
        }
        """;
        return BinaryData.FromString(eventData).ToArray();
    }
}
