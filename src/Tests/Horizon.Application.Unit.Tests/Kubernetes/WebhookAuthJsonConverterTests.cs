using System;
using System.Buffers;
using System.Text.Json;
using FluentAssertions;
using Horizon.Application.Kubernetes;
using Moq;
using Xunit;

namespace Horizon.Application.Tests.Kubernetes;

public class WebhookAuthJsonConverterTests
{
    [Fact]
    public void CanConvert_ShouldReturnTrue_WhenTypeToConvertIsAssignable()
    {
        // Arrange
        var converter = new WebhookAuthJsonConverter();
        var typeToConvert = typeof(WebhookAuthenticationNone);

        // Act
        var result = converter.CanConvert(typeToConvert);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanConvert_ShouldReturnFalse_WhenTypeToConvertIsNotAssignable()
    {
        // Arrange
        var converter = new WebhookAuthJsonConverter();
        var typeToConvert = typeof(object);

        // Act
        var result = converter.CanConvert(typeToConvert);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Read_ShouldThrowJsonException_WhenTokenTypeIsNotStartObject()
    {
        // Arrange
        var converter = new WebhookAuthJsonConverter();

        // Act & Assert
        converter.Invoking(c => {
            var tempReader = new Utf8JsonReader([]);
            c.Read(ref tempReader, typeof(WebhookAuthentication), new JsonSerializerOptions());
        }).Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_ShouldThrowJsonException_WhenDiscriminatorPropertyIsMissing()
    {
        // Arrange
        var converter = new WebhookAuthJsonConverter();
        var reader = new Utf8JsonReader("{"u8);

        // Act & Assert
        converter.Invoking(c => {
            var tempReader = new Utf8JsonReader([]);
            c.Read(ref tempReader, typeof(WebhookAuthentication), new JsonSerializerOptions());
        }).Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_ShouldThrowNotSupportedException_WhenWebhookAuthenticationTypeIsNotSupported()
    {
        // Arrange
        var converter = new WebhookAuthJsonConverter();

        // Act & Assert
        converter.Invoking(c => {
            var reader = new Utf8JsonReader("""{ "type" : "invalid" }"""u8);
            reader.Read();
            c.Read(ref reader, typeof(WebhookAuthentication), new JsonSerializerOptions());
        }).Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Read_ShouldDeserializeWebhookAuthenticationNone_WhenDiscriminatorIsWebhookAuthenticationNone()
    {
        // Arrange
        var converter = new WebhookAuthJsonConverter();
        var reader = new Utf8JsonReader("{\"type\":\"None\"}"u8);
        reader.Read();
        var options = new JsonSerializerOptions();

        // Act
        var result = converter.Read(ref reader, typeof(WebhookAuthentication), options);

        // Assert
        result.Should().BeOfType<WebhookAuthenticationNone>();
    }

    [Fact]
    public void Read_ShouldDeserializeWebhookAuthenticationBasic_WhenDiscriminatorIsWebhookAuthenticationBasic()
    {
        // Arrange
        var converter = new WebhookAuthJsonConverter();
        var reader = new Utf8JsonReader("{\"type\":\"Basic\"}"u8);
        reader.Read();
        var options = new JsonSerializerOptions();

        // Act
        var result = converter.Read(ref reader, typeof(WebhookAuthentication), options);

        // Assert
        result.Should().BeOfType<WebhookAuthenticationBasic>();
    }

    [Fact]
    public void Read_ShouldDeserializeWebhookAuthenticationAzureAD_WhenDiscriminatorIsWebhookAuthenticationAzureAD()
    {
        // Arrange
        var converter = new WebhookAuthJsonConverter();
        var reader = new Utf8JsonReader("{\"type\":\"AzureAD\"}"u8);
        reader.Read();
        var options = new JsonSerializerOptions();

        // Act
        var result = converter.Read(ref reader, typeof(WebhookAuthentication), options);

        // Assert
        result.Should().BeOfType<WebhookAuthenticationAzureAD>();
    }

    [Fact]
    public void Write_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var converter = new WebhookAuthJsonConverter();
        var mockBufferWriter = new Mock<IBufferWriter<byte>>();
        var writer = new Utf8JsonWriter(mockBufferWriter.Object);
        var value = new WebhookAuthenticationNone();
        var options = new JsonSerializerOptions();

        // Act & Assert
        converter.Invoking(c => c.Write(writer, value, options))
            .Should().Throw<InvalidOperationException>();
    }
}
