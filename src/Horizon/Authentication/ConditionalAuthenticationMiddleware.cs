using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Horizon.Authentication;

public class ConditionalAuthenticationMiddleware(
    RequestDelegate next,
    AuthenticationConfigProvider configProvider)
{
    public const string ProbeUrl = "/probe";

    public async Task InvokeAsync(HttpContext context)
    {
        if(context.Request?.Path.Value?.Equals(ProbeUrl, System.StringComparison.InvariantCultureIgnoreCase) ?? false)
        {
            await next(context);
            return;
        }
        var auth = configProvider.Get<AuthenticationBase>();
        switch(auth)
        {
            case NoAuthentication:
                await next(context);
                break;
            default:
                var result = await context.AuthenticateAsync(auth.Type);
                if (result.Succeeded)
                {
                    await next(context);
                }
                else
                {
                    await context.ChallengeAsync(auth.Type);
                }
                break;
        }
    }
}