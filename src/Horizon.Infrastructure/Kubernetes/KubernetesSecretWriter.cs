using System.Collections.Generic;
using System.Text;
using System;
using System.Threading.Tasks;
using ErrorOr;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace Horizon.Infrastructure.Kubernetes;

public class KubernetesSecretWriter(
    IKubernetes client,
    ILogger<KubernetesSecretWriter> logger)
{
    internal const string DefaultSecretType = "Opaque";

    public async Task<ErrorOr<Success>> ReplaceAsync(string secretName, string @namespace)
    {
        // Define the Secret data
        var secretData = new Dictionary<string, byte[]>
        {
            { "username", Encoding.UTF8.GetBytes("my-username") },
            { "password", Encoding.UTF8.GetBytes("my-password") }
        };

        // Create the Secret object
        var secret = new V1Secret
        {
            Metadata = new V1ObjectMeta
            {
                Name = secretName,
                NamespaceProperty = @namespace
            },
            Data = secretData,
            Type = DefaultSecretType // Opaque is the default secret type
        };

        // Create the Secret in Kubernetes
        try
        {
            var updatedSecret = await client.CoreV1.ReplaceNamespacedSecretAsync(secret, "my-secret", "default");
            logger.LogInformation("SecretUpdated: {Name} {Namespace}", updatedSecret.Metadata.Name, updatedSecret.Metadata.NamespaceProperty);
            return Result.Success;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ErrorReplacingSecret");
            return Error.Unexpected(description: exception.Message);
        }
    }
}
