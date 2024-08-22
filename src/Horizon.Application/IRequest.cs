namespace Horizon.Application;

public interface IRequest<TResponse> where TResponse : notnull, new()
{
}
