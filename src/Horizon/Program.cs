using Horizon;
using Horizon.Application;
using Horizon.Infrastructure;
using Horizon.UseCases;
using k8s;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddHostedService<HostedService>();
builder.Services.AddScoped<WebhookDeliveryHandler>();
builder.Services.AddScoped<WebhookValidationHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer();

using var app = builder.Build();
app.UseOpenApi();
app.UseSwaggerUi();
app.MapPost("/webhook", CreateLambdaForHandler<WebhookDeliveryHandler>());
app.MapMethods("/webhook", ["OPTIONS"], CreateLambdaForHandler<WebhookValidationHandler>());
app.Run();

static RequestDelegate CreateLambdaForHandler<T>() where T : class, IApiHandler =>
    async context =>
    {
        var handler = context.RequestServices.GetRequiredService<T>();
        var httpRequest = context.Request;
        var cancellationToken = context.RequestAborted;
        var result = await handler.HandleAsync(httpRequest, cancellationToken);
        await result.ExecuteAsync(context);
    };