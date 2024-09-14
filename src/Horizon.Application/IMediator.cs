using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace Horizon.Application;

public interface IMediator
{
    Task<ErrorOr<TResponse>> SendAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<ErrorOr<TResponse>>
        where TResponse : notnull, new();
}