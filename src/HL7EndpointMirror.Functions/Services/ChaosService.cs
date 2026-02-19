using HL7EndpointMirror.Functions.Models;

namespace HL7EndpointMirror.Functions.Services;

public class ChaosService : IChaosService
{
    private readonly IRandomProvider _randomProvider;

    public ChaosService(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider;
    }

    public Task<ChaosDecision> EvaluateAsync(ChaosConfig config)
    {
        // Chaos disabled — always pass, check latency only
        if (!config.IsEnabled)
        {
            var passDelay = GetDelayMs(config);
            return Task.FromResult(ChaosDecision.Pass(passDelay));
        }

        // Roll 0–99 inclusive. If roll < FailureRatePercent, fail.
        // e.g. 25% failure rate: roll 0–24 = fail, 25–99 = pass
        var roll = _randomProvider.Next(0, 100);
        var shouldFail = roll < config.FailureRatePercent;


        if (shouldFail)
        {
            var statusCode = ParseStatusCode(config.DefaultErrorType);
            var failDelay = GetDelayMs(config);
            return Task.FromResult(ChaosDecision.Fail(statusCode, failDelay));
        }

        var delay = GetDelayMs(config);
        return Task.FromResult(ChaosDecision.Pass(delay));
    }

    private static int GetDelayMs(ChaosConfig config) =>
        config.LatencySimulation.IsEnabled
            ? config.LatencySimulation.DelayMs
           : 0;
    private static int ParseStatusCode(string errorType) =>
        int.TryParse(errorType, out var code) ? code : 500;
}


