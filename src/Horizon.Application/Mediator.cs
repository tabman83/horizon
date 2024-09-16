using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using Horizon.Application.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Horizon.Application;

public class Mediator(
    IServiceProvider serviceProvider,
    ILogger<Mediator> logger) : IMediator
{
    public async Task<ErrorOr<TResponse>> SendAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<ErrorOr<TResponse>>
        where TResponse : notnull, new()
    {
        using (logger.With("RequestName", request.GetType().Name).BeginScope())
        {
            try
            {
                logger.LogInformation("RequestStarted");
                var handler = (IAsyncRequestHandler<TRequest, TResponse>)serviceProvider.GetRequiredService(typeof(IAsyncRequestHandler<TRequest, TResponse>));
                return await handler.HandleAsync(request, cancellationToken);
            }
            catch(Exception exception)
            {
                logger.LogError(exception, "RequestError");
                return Error.Unexpected();
            }
            finally
            {
                logger.LogInformation("RequestCompleted");
            }
        }
    }
}