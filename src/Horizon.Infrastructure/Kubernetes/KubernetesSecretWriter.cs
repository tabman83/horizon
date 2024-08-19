using System.Collections.Generic;
using System.Text;
using System;
using System.Threading.Tasks;
using ErrorOr;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Horizon.Infrastructure.Kubernetes;

public class KubernetesSecretWriter(
    IKubernetes client,
    ILogger<KubernetesSecretWriter> logger) : IKubernetesSecretWriter
{
    internal const string DefaultSecretType = "Opaque";

    public async Task<ErrorOr<Success>> EnsureExistsAsync(string kubernetesSecretName, string @namespace, CancellationToken cancellationToken)
    {
        try
        {
            await client.CoreV1.ReadNamespacedSecretAsync(kubernetesSecretName, @namespace, cancellationToken: cancellationToken);
            return Result.Success;
        }
        catch (HttpOperationException e) when (e.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            try
            {
                var secret = new V1Secret
                {
                    Metadata = new V1ObjectMeta
                    {
                        Name = kubernetesSecretName,
                        NamespaceProperty = @namespace
                    },
                    Data = new Dictionary<string, byte[]> { },
                    Type = DefaultSecretType
                };
                await client.CoreV1.CreateNamespacedSecretAsync(secret, @namespace, cancellationToken: cancellationToken);
                logger.LogInformation("SecretCreated: {Name} {Namespace}", kubernetesSecretName, @namespace);
                return Result.Success;
            }
            catch(Exception createException)
            {
                logger.LogError(createException, "ErrorCreatingSecret");
                return Error.Unexpected(description: createException.Message);
            }
        }
        catch (Exception readException)
        {
            logger.LogError(readException, "ErrorReadingSecret");
            return Error.Unexpected(description: readException.Message);
        }
    }

    public async Task<ErrorOr<Success>> PatchAsync(string kubernetesSecretName, string secretName, string @namespace, string secretValue, CancellationToken cancellationToken)
    {
        try
        {
            // Define the patch data (strategic merge patch)
            var patch = new V1Secret
            {
                Metadata = new V1ObjectMeta
                {
                    Name = kubernetesSecretName,
                    NamespaceProperty = @namespace
                },
                Data = new Dictionary<string, byte[]>
                {
                    { secretName, Encoding.UTF8.GetBytes(secretValue) }
                }
            };
            // Apply the patch
            var patchedSecret = await client.CoreV1.PatchNamespacedSecretAsync(new V1Patch(patch, V1Patch.PatchType.StrategicMergePatch), name: kubernetesSecretName, namespaceParameter: @namespace, cancellationToken: cancellationToken);

            logger.LogInformation("SecretPatched: {Name} {Namespace} {Secret}", kubernetesSecretName, @namespace, secretName);
            return Result.Success;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ErrorReplacingSecret");
            return Error.Unexpected(description: exception.Message);
        }
    }
}