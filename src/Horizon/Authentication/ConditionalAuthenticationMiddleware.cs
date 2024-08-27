using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Horizon.Authentication;

public class ConditionalAuthenticationMiddleware(
    RequestDelegate next,
    AuthenticationConfigProvider configProvider)
{
    public async Task InvokeAsync(HttpContext context)
    {
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