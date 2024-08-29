using System.Threading.Tasks;
using Horizon.Application.Kubernetes;
using Horizon.Authentication;
using Horizon.Infrastructure.Kubernetes.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Horizon.Reconciliators;

public class ConfigurationReconciliator(
    ILogger<HostedService> logger,
    AuthenticationConfigProvider configProvider,
    IAuthenticationSchemeProvider authSchemeProvider) : IReconciliator<HorizonProviderConfigurationObject>
{
    public Task ReconcileAsync(WatchEventType type, HorizonProviderConfigurationObject item)
    {
        if (item.Spec is null)
        {
            logger.LogError("HorizonProviderConfigurationSpec is null");
            return Task.CompletedTask;
        }
        var spec = item.Spec;
        authSchemeProvider.RemoveScheme(spec.WebhookAuthentication.Type);
        configProvider.Set(new NoAuthentication());
        switch (spec.WebhookAuthentication)
        {
            case WebhookAuthenticationNone:
                break;
            case WebhookAuthenticationBasic auth:
                configProvider.Set(new BasicAuthentication(auth.Username, auth.Password));
                authSchemeProvider.AddScheme(new AuthenticationScheme(auth.Type, auth.Type, typeof(BasicAuthenticationHandler)));
                break;
            case WebhookAuthenticationAzureAD auth:
                configProvider.Set(new AzureAdAuthentication(auth.TenantId, auth.ClientId));
                authSchemeProvider.AddScheme(new AuthenticationScheme(auth.Type, auth.Type, typeof(AzureAdAuthenticationHandler)));
                break;
            default:
                logger.LogError("HorizonProviderConfigurationSpec is invalid");
                break;
        }
        return Task.CompletedTask;
    }
}
