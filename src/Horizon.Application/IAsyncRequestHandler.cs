using System.Threading;
using System.Threading.Tasks;

namespace Horizon.Application;

public interface IAsyncRequestHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
