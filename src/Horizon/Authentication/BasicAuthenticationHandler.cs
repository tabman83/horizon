using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.Net.Http.Headers;

namespace Horizon.Authentication;

public class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    AuthenticationConfigProvider configProvider,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string MissingAuthorizationHeaderMessage = "Missing Authorization Header";
    internal const string InvalidAuthorizationHeaderMessage = "Invalid Authorization Header";
    internal const string InvalidUsernameOrPasswordMessage = "Invalid Username or Password";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        await Task.Yield();

        if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out Microsoft.Extensions.Primitives.StringValues value))
        {
            return AuthenticateResult.Fail(MissingAuthorizationHeaderMessage);
        }

        if(!AuthenticationHeaderValue.TryParse(value, out var authHeader))
        {
            return AuthenticateResult.Fail(InvalidAuthorizationHeaderMessage);
        }

        try
        {
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];
            var password = credentials[1];

            // Validate username and password here (e.g., against a database)
            if (IsValidUser(username, password, configProvider.Get<BasicAuthentication>()!))
            {
                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail("Invalid Username or Password");
            }
        }
        catch
        {
            return AuthenticateResult.Fail(InvalidAuthorizationHeaderMessage);
        }
    }

    private static bool IsValidUser(string username, string password, BasicAuthentication authentication)
    {
        return username == authentication.Username && password == authentication.Password;
    }
}
