using System;
using System.Diagnostics.CodeAnalysis;
using Horizon.Application.Kubernetes;

namespace Horizon;

public abstract record AuthenticationBase(string Type);
public sealed record class NoAuthentication() : AuthenticationBase(WebhookAuthenticationNone.AuthType);
public sealed record class BasicAuthentication(string Username, string Password) : AuthenticationBase(WebhookAuthenticationBasic.AuthType);
public sealed record class AzureAdAuthentication(string TenantId, string ClientId) : AuthenticationBase(WebhookAuthenticationAzureAD.AuthType);

[ExcludeFromCodeCoverage]
public class AuthenticationConfigProvider
{
    public AuthenticationBase _authentication = new NoAuthentication();

    public virtual void Set(AuthenticationBase authentication)
    {
        _authentication = authentication;
    }

    public virtual T Get<T>() where T : AuthenticationBase
    {
        return _authentication as T ?? throw new InvalidOperationException();
    }
}