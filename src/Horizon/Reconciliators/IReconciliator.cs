using System.Threading.Tasks;
using Horizon.Application.Kubernetes;

namespace Horizon.Reconciliators;

public interface IReconciliator<T>
{
    Task ReconcileAsync(WatchEventType type, T item);
}