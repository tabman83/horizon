using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Horizon.Application;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> SendAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
    {
        var handler = (IAsyncRequestHandler<TRequest, TResponse>)serviceProvider.GetRequiredService(typeof(IAsyncRequestHandler<TRequest, TResponse>));
        return await handler.HandleAsync(request, cancellationToken);
    }
}
