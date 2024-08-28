using System.Diagnostics.CodeAnalysis;
using Horizon;
using Horizon.Application;
using Horizon.Authentication;
using Horizon.Infrastructure;
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<AuthenticationConfigProvider>();

using var app = builder.Build();
app.UseOpenApi();
app.UseSwaggerUi();
app.UseMiddleware<ConditionalAuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapPost(webhooksUrl, CreateLambdaForHandler<WebhookDeliveryHandler>())
    .WithOpenApi();
app.MapMethods(webhooksUrl, ["OPTIONS"], CreateLambdaForHandler<WebhookValidationHandler>())
    .WithOpenApi();
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
public partial class Program { }