namespace Horizon.Application;

public interface IRequest<in TResponse> where TResponse : notnull, new()
{
}
