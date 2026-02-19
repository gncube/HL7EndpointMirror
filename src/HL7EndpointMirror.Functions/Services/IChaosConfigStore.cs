using HL7EndpointMirror.Functions.Models;

namespace HL7EndpointMirror.Functions.Services;

public interface IChaosConfigStore
{
    Task<ChaosConfig> GetAsync();
    Task SaveAsync(ChaosConfig config);
}