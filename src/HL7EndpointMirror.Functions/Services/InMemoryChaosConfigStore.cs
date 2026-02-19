using HL7EndpointMirror.Functions.Models;

using Microsoft.Extensions.Options;

namespace HL7EndpointMirror.Functions.Services;

public class InMemoryChaosConfigStore : IChaosConfigStore
{
    private ChaosConfig _current;

    public InMemoryChaosConfigStore(IOptions<ChaosConfig> options)
    {
        // Seed from bound configuration (local.settings.json / env vars)
        _current = options.Value;
    }

    public Task<ChaosConfig> GetAsync() =>
        Task.FromResult(_current);

    public Task SaveAsync(ChaosConfig config)
    {
        _current = config;
        return Task.CompletedTask;
    }
}