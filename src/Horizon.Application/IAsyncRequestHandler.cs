using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace Horizon.Application;

public interface IAsyncRequestHandler<TRequest, TResponse>
{
    Task<ErrorOr<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
