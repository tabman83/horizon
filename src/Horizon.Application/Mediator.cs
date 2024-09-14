using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
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
        using (logger.BeginScope(new Dictionary<string, object> { ["RequestName"] = request.GetType().Name }))
        {
            try
            {
                logger.LogInformation("RequestStarted");
                var handler = (IAsyncRequestHandler<TRequest, TResponse>)serviceProvider.GetRequiredService(typeof(IAsyncRequestHandler<TRequest, TResponse>));
                return await handler.HandleAsync(request, cancellationToken);
            }
            catch
            {
                logger.LogInformation("RequestError");
                return Error.Unexpected();
            }
            finally
            {
                logger.LogInformation("RequestCompleted");
            }
        }
    }
}