using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;

namespace Horizon.UseCases;

public interface IApiHandler
{
    Task<IResult> HandleAsync(HttpRequest httpRequest, CancellationToken cancellationToken = default);
}
