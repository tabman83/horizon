using System.Text.Json;
using System;
using System.Text.Json.Serialization;

namespace Horizon.Application.Kubernetes;

public class WebhookAuthJsonConverter : JsonConverter<WebhookAuthentication>
{
    internal const string Discriminator = "type";
    internal readonly Type WebhookAuthenticationType = typeof(WebhookAuthentication);

    public override bool CanConvert(Type typeToConvert)
    {
        return WebhookAuthenticationType.IsAssignableFrom(typeToConvert);
    }

    public override WebhookAuthentication? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        if (!jsonDocument.RootElement.TryGetProperty(Discriminator, out var typeProperty))
        {
            throw new JsonException();
        }
        var auth = jsonDocument.RootElement.GetRawText();

        // Deserialize based on the discriminator value
        return typeProperty.GetString() switch
        {
            WebhookAuthenticationNone.AuthType => JsonSerializer.Deserialize<WebhookAuthenticationNone>(auth, options),
            WebhookAuthenticationBasic.AuthType => JsonSerializer.Deserialize<WebhookAuthenticationBasic>(auth, options),
            WebhookAuthenticationAzureAD.AuthType => JsonSerializer.Deserialize<WebhookAuthenticationAzureAD>(auth, options),
            _ => throw new NotSupportedException($"Webhook authentication type '{typeProperty}' is not supported"),
        };
    }

    public override void Write(Utf8JsonWriter writer, WebhookAuthentication value, JsonSerializerOptions options)
    {
        throw new InvalidOperationException("Serialization is not supported");
    }
}