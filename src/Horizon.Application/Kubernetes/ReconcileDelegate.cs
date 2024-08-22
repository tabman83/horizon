using System.Collections.Generic;
using System.Threading.Tasks;

namespace Horizon.Application.Kubernetes;

public delegate Task ReconcileDelegate(WatchEventType type, string? name, string? @namespace, IEnumerable<AzureKeyVaultSubscriptionSpec> items);