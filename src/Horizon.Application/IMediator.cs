using System.Threading.Tasks;

namespace Horizon.Application;

public interface IMediator
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse>;
}