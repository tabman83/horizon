using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Horizon.Application;
using Horizon.Infrastructure.Kubernetes;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Horizon.Infrastructure.Tests.Kubernetes;

public class KubernetesSecretWriterTests
{
    private readonly Mock<ICoreV1Operations> _v1OperationsMock;
    private readonly Mock<IKubernetes> _mockClient;
    private readonly Mock<ILogger<KubernetesSecretWriter>> _mockLogger;
    private readonly KubernetesSecretWriter _secretWriter;

    public KubernetesSecretWriterTests()
    {
        _v1OperationsMock = new Mock<ICoreV1Operations>();
        _mockClient = new Mock<IKubernetes>();
        _mockClient.SetupGet(x => x.CoreV1).Returns(_v1OperationsMock.Object);
        _mockLogger = new Mock<ILogger<KubernetesSecretWriter>>();
        _secretWriter = new KubernetesSecretWriter(_mockClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReplaceSecret_WhenSecretExists()
    {
        // Arrange
        var secretName = "my-secret";
        var @namespace = "my-namespace";
        var secrets = new Dictionary<string, string>
        {
            { "username", "admin" },
            { "password", "password123" }
        };

        _v1OperationsMock.Setup(x => x.ReplaceNamespacedSecretWithHttpMessagesAsync(It.IsAny<V1Secret>(), secretName, @namespace, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null, default))
            .ReturnsAsync(new HttpOperationResponse<V1Secret>())
            .Verifiable();

        // Act
        var result = await _secretWriter.ReplaceAsync(secretName, @namespace, secrets.Select(kv => new SecretBundle(kv.Key, kv.Value)), cancellationToken: default);

        // Assert
        result.IsError.Should().BeFalse();
        _v1OperationsMock.Verify();
    }

    [Fact]
    public async Task ReplaceAsync_ShouldCreateSecret_WhenSecretDoesNotExist()
    {
        // Arrange
        var secretName = "my-secret";
        var @namespace = "my-namespace";
        var secrets = new Dictionary<string, string>
        {
            { "username", "admin" },
            { "password", "password123" }
        };

        _v1OperationsMock.Setup(x => x.ReplaceNamespacedSecretWithHttpMessagesAsync(It.IsAny<V1Secret>(), secretName, @namespace, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null, default))
            .ThrowsAsync(new HttpOperationException { Response = new k8s.Autorest.HttpResponseMessageWrapper(new HttpResponseMessage(HttpStatusCode.NotFound), string.Empty) })
            .Verifiable();

        _v1OperationsMock.Setup(x => x.CreateNamespacedSecretWithHttpMessagesAsync(It.IsAny<V1Secret>(), @namespace, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null, default))
            .ReturnsAsync(new HttpOperationResponse<V1Secret>())
            .Verifiable();

        // Act
        var result = await _secretWriter.ReplaceAsync(secretName, @namespace, secrets.Select(kv => new SecretBundle(kv.Key, kv.Value)));

        // Assert
        result.IsError.Should().BeFalse();
        _v1OperationsMock.Verify();
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnError_WhenReplaceThrowsException()
    {
        // Arrange
        var secretName = "my-secret";
        var @namespace = "my-namespace";
        var secrets = new Dictionary<string, string>
        {
            { "username", "admin" },
            { "password", "password123" }
        };

        _v1OperationsMock.Setup(x => x.ReplaceNamespacedSecretWithHttpMessagesAsync(It.IsAny<V1Secret>(), secretName, @namespace, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null, default))
            .Throws(new Exception("Replace error"));

        // Act
        var result = await _secretWriter.ReplaceAsync(secretName, @namespace, secrets.Select(kv => new SecretBundle(kv.Key, kv.Value)));

        // Assert
        result.IsError.Should().BeTrue();
        _v1OperationsMock.Verify();
    }

    [Fact]
    public async Task PatchAsync_ShouldPatchSecret()
    {
        // Arrange
        var secretName = "my-secret";
        var @namespace = "my-namespace";

        _v1OperationsMock.Setup(x => x.PatchNamespacedSecretWithHttpMessagesAsync(It.IsAny<V1Patch>(), secretName, @namespace, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null, null, default))
            .ReturnsAsync(new HttpOperationResponse<V1Secret>())
            .Verifiable();

        // Act
        var result = await _secretWriter.PatchAsync(secretName, @namespace, new SecretBundle("username", "admin"));

        // Assert
        result.IsError.Should().BeFalse();
        _v1OperationsMock.Verify();
    }

    [Fact]
    public async Task PatchAsync_ShouldReturnError_WhenPatchThrowsException()
    {
        // Arrange
        var secretName = "my-secret";
        var @namespace = "my-namespace";

        _v1OperationsMock.Setup(x => x.PatchNamespacedSecretWithHttpMessagesAsync(It.IsAny<V1Patch>(), secretName, @namespace, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null, null, default))
            .Throws(new Exception("Patch error"));

        // Act
        var result = await _secretWriter.PatchAsync(secretName, @namespace, new SecretBundle("username", "admin"));

        // Assert
        result.IsError.Should().BeTrue();
        _v1OperationsMock.Verify();
    }
}
