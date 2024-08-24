using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Horizon.Authentication;

public class DynamicAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemeProvider)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Example: Add a new OAuth scheme dynamically
        var schemeName = "DynamicOAuth";
        var existingScheme = await schemeProvider.GetSchemeAsync(schemeName);
        if (existingScheme == null)
        {
            //await schemeProvider.AddSchemeAsync(new AuthenticationScheme(
            //    schemeName,
            //    schemeName,
            //    typeof(DynamicOAuthHandler)));
        }

        await next(context);
    }
}
