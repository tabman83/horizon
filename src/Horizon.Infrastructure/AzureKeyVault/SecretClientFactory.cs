using System;
using System.Collections.Concurrent;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;

namespace Horizon.Infrastructure.AzureKeyVault;

public class SecretClientFactory
{
    private readonly TokenCredential _credential = null!;
    private readonly ConcurrentDictionary<string, SecretClient> _clientsCache = new();

    protected SecretClientFactory()
    {
    }

    public SecretClientFactory(TokenCredential credential)
    {
        _credential = credential;
    }

    public virtual SecretClient CreateClient(string keyVaultName) =>
        _clientsCache.GetOrAdd(keyVaultName, keyVaultName => new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net"), _credential));
}
