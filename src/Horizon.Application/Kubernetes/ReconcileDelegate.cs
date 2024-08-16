using System.Collections.Generic;

namespace Horizon.Application.Kubernetes;

public delegate void ReconcileDelegate(WatchEventType type, string? name, string? @namespace, IEnumerable<AzureKeyVaultSubscriptionSpec> items);