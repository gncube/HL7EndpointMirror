namespace HL7EndpointMirror.Functions.Models;

public class ChaosDecision
{
    public bool ShouldFail { get; init; }
    public int StatusCode { get; init; }
    public int DelayMs { get; init; }

    public static ChaosDecision Pass(int delayMs = 0) => new()
    {
        ShouldFail = false,
        StatusCode = 200,
        DelayMs = delayMs
    };

    public static ChaosDecision Fail(int statusCode, int delayMs = 0) => new()
    {
        ShouldFail = true,
        StatusCode = statusCode,
        DelayMs = delayMs
    };
}