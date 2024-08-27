using Azure.Security.KeyVault.Secrets;
using Azure;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Horizon.Infrastructure.AzureKeyVault;

internal class ParallelSecretLoader(SecretClient client) : IDisposable
{
    private const int ParallelismLevel = 32;
    private readonly SecretClient _client = client;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(ParallelismLevel, ParallelismLevel);
    private readonly List<Task<Response<KeyVaultSecret>>> _tasks = [];

    public void AddSecretToLoad(string secretName)
    {
        _tasks.Add(Task.Run(() => GetSecretAsync(secretName)));
    }

    private async Task<Response<KeyVaultSecret>> GetSecretAsync(string secretName)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            return await _client.GetSecretAsync(secretName).ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task<Response<KeyVaultSecret>[]> WaitForAllAsync() => Task.WhenAll(_tasks);

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}
