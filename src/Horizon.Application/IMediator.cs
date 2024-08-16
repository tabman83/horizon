using System.Threading;
using System.Threading.Tasks;

namespace Horizon.Application;

public interface IMediator
{
    Task<TResponse> SendAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>;
}