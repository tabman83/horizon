using System.Collections.Generic;
using System.Text;
using System;
using System.Threading.Tasks;
using ErrorOr;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using System.Threading;
using Horizon.Application.Kubernetes;
using Horizon.Application;
using System.Linq;
using k8s.Autorest;

namespace Horizon.Infrastructure.Kubernetes;

public class KubernetesSecretWriter(
    IKubernetes client,
    ILogger<KubernetesSecretWriter> logger) : IKubernetesSecretWriter
{
    internal const string DefaultSecretType = "Opaque";
    internal const string ManagedByLabelName = "app.kubernetes.io/managed-by";
    internal const string ManagedByLabelValue = "Horizon";

    public async Task<ErrorOr<Success>> ReplaceAsync(string kubernetesSecretObjectName, string @namespace, IEnumerable<SecretBundle> secrets, CancellationToken cancellationToken = default)
    {
        var secret = new V1Secret
        {
            Metadata = new V1ObjectMeta 
            { 
                Name = kubernetesSecretObjectName, 
                NamespaceProperty = @namespace,
                Labels = new Dictionary<string, string>
                {
                    [ManagedByLabelName] = "Horizon"
                }
            },
            Data = secrets.ToDictionary(s => s.Name, s => Encoding.UTF8.GetBytes(s.Value)),
            Type = DefaultSecretType
        };

        try
        {
            await client.CoreV1.ReplaceNamespacedSecretAsync(secret, kubernetesSecretObjectName, @namespace, cancellationToken: cancellationToken);
            return Result.Success;
        }
        catch (HttpOperationException exception) when (exception.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // If the secret does not exist, create it
            try
            {
                await client.CoreV1.CreateNamespacedSecretAsync(secret, @namespace, cancellationToken: cancellationToken);
                return Result.Success;
            }
            catch (Exception nestedException)
            {
                logger.LogError(nestedException, "ErrorCreatingSecret");
                return Error.Unexpected(description: nestedException.Message);
            }
        }
        catch(Exception exception)
        {
            logger.LogError(exception, "ErrorReplacingSecret");
            return Error.Unexpected(description: exception.Message);
        }
    }

    public async Task<ErrorOr<Success>> PatchAsync(string kubernetesSecretObjectName, string @namespace, SecretBundle secret, CancellationToken cancellationToken = default)
    {
        try
        {
            // Define the patch data (strategic merge patch)
            var patch = new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = kubernetesSecretObjectName,
                    NamespaceProperty = @namespace
                },
                Data = new Dictionary<string, byte[]>
                {
                    { secret.Name, Encoding.UTF8.GetBytes(secret.Value) }
                }
            };
            // Apply the patch
            var patchedSecret = await client.CoreV1.PatchNamespacedSecretAsync(new V1Patch(patch, V1Patch.PatchType.StrategicMergePatch), name: kubernetesSecretObjectName, namespaceParameter: @namespace, cancellationToken: cancellationToken);

            logger.LogInformation("SecretPatched: {Name} {Namespace} {Secret}", kubernetesSecretObjectName, @namespace, secret.Name);
            return Result.Success;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ErrorReplacingSecret");
            return Error.Unexpected(description: exception.Message);
        }
    }
}