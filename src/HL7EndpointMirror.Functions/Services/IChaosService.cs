using HL7EndpointMirror.Functions.Models;

namespace HL7EndpointMirror.Functions.Services;

public interface IChaosService
{
    Task<ChaosDecision> EvaluateAsync(ChaosConfig config);
}