using System.Threading.Tasks;

namespace Horizon.Application.Kubernetes;

public delegate Task ReconcileDelegate<in T>(WatchEventType type, T item);