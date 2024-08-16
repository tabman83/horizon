using System;
using System.Collections.Concurrent;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;

namespace Horizon.Infrastructure.AzureKeyVault;

public class SecretClientFactory
{
    private readonly TokenCredential _credential = null!;
    private readonly ConcurrentDictionary<Uri, SecretClient> _clientsCache = new();

    protected SecretClientFactory()
    {
    }

    public SecretClientFactory(TokenCredential credential)
    {
        _credential = credential;
    }

    public virtual SecretClient CreateClient(Uri keyVaultUri) =>
        _clientsCache.GetOrAdd(keyVaultUri, uri => new SecretClient(uri, _credential));
}
