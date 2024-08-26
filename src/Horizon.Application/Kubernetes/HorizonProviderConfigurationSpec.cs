using System.Text.Json.Serialization;

namespace Horizon.Application.Kubernetes;

public record HorizonProviderConfigurationSpec(AzureKeyVaultAuthentication AzureKeyVaultAuthentication, WebhookAuthentication WebhookAuthentication);

public record AzureKeyVaultAuthentication(string Type);

[JsonConverter(typeof(WebhookAuthJsonConverter))]
public abstract record WebhookAuthentication(string Type);

public record WebhookAuthenticationNone() : WebhookAuthentication(AuthType)
{
    public const string AuthType = "None";
}

public record WebhookAuthenticationBasic(string Username, string Password) : WebhookAuthentication(AuthType)
{
    public const string AuthType = "Basic";
}
public record WebhookAuthenticationAzureAD(string TenantId, string ClientId) : WebhookAuthentication(AuthType)
{
    public const string AuthType = "AzureAD";
}