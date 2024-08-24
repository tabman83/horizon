using System.Threading.Tasks;

namespace Horizon.Application.Kubernetes;

public delegate Task ReconcileDelegate<T>(WatchEventType type, T item);