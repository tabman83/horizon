using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System;
using Microsoft.Net.Http.Headers;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http.Headers;

namespace Horizon.Authentication;

public class AzureAdAuthenticationHandler(
    IOptionsMonitor<OpenIdConnectOptions> options,
    ILoggerFactory logger,
    AuthenticationConfigProvider configProvider,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string MissingAuthorizationHeaderMessage = "Missing Authorization Header";
    internal const string InvalidAuthorizationHeaderMessage = "Invalid Authorization Header";
    internal const string InvalidTokenMessage = "Invalid token";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out Microsoft.Extensions.Primitives.StringValues value))
        {
            return AuthenticateResult.Fail(MissingAuthorizationHeaderMessage);
        }

        if (!AuthenticationHeaderValue.TryParse(value, out var authHeader))
        {
            return AuthenticateResult.Fail(InvalidAuthorizationHeaderMessage);
        }

        try
        {
            // Validate the extracted token
            var claimsPrincipal = await ValidateTokenAsync(authHeader.Parameter!, default);

            // Create an authentication ticket
            var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception exception)
        {
            // Log the exception and fail the authentication
            Logger.LogError(exception, "Token validation failed");
            return AuthenticateResult.Fail(InvalidTokenMessage);
        }
    }

    private async Task<ClaimsPrincipal> ValidateTokenAsync(string token, CancellationToken cancellationToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var authConfig = configProvider.Get<AzureAdAuthentication>();
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = $"https://sts.windows.net/{authConfig!.TenantId}/", // options.Authorit & Tenant ID
            ValidAudience = authConfig.ClientId, // Your application's Client ID
            IssuerSigningKeys = await GetIssuerSigningKeysAsync(authConfig.TenantId, cancellationToken),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        var principal = handler.ValidateToken(token, tokenValidationParameters, out SecurityToken _);
        return principal;
    }

    private static async Task<IEnumerable<SecurityKey>> GetIssuerSigningKeysAsync(string tenantId, CancellationToken cancellationToken)
    {
        var metadataAddress = $"https://sts.windows.net/{tenantId}/.well-known/openid-configuration";
        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever());

        var openIdConfig = await configurationManager.GetConfigurationAsync(cancellationToken);
        return openIdConfig.SigningKeys;
    }
}