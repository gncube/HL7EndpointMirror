namespace HL7EndpointMirror.Functions.Models;

public class RequestLogEntry
{
    public string MessageId { get; init; } = string.Empty;
    public string MessageControlId { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public long ProcessingTimeMs { get; init; }
    public bool ChaosApplied { get; init; }
    public string? ChaosType { get; init; }
    public string AckCode { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}