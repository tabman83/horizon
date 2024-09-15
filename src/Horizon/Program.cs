using System.Diagnostics.CodeAnalysis;
using Horizon;
using Horizon.Application;
using Horizon.Authentication;
using Horizon.Infrastructure;
using Horizon.Reconciliators;
using Horizon.UseCases;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

const string webhooksUrl = "/webhooks";

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddHostedService<HostedService>();
builder.Services.AddScoped<WebhookDeliveryHandler>();
builder.Services.AddScoped<WebhookValidationHandler>();
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<AuthenticationConfigProvider>();
builder.Services.AddReconciliators();

using var app = builder.Build();
app.UseMiddleware<ConditionalAuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapPost(webhooksUrl, CreateLambdaForHandler<WebhookDeliveryHandler>());
app.MapMethods(webhooksUrl, ["OPTIONS"], CreateLambdaForHandler<WebhookValidationHandler>());
app.MapMethods("/probe", ["HEAD"], () => Results.Ok("Healthy"));
await app.RunAsync();

static RequestDelegate CreateLambdaForHandler<T>() where T : class, IApiHandler =>
    async context =>
    {
        var handler = context.RequestServices.GetRequiredService<T>();
        var httpRequest = context.Request;
        var cancellationToken = context.RequestAborted;
        var result = await handler.HandleAsync(httpRequest, cancellationToken);
        await result.ExecuteAsync(context);
    };

[ExcludeFromCodeCoverage]
public static partial class Program { }