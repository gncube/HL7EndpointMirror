namespace HL7EndpointMirror.Functions.Models;

public class Hl7ParseResult
{
    public bool IsValid { get; init; }
    public string MessageControlId { get; init; } = string.Empty;
    public string? ErrorReason { get; init; }

    public static Hl7ParseResult Success(string messageControlId) => new()
    {
        IsValid = true,
        MessageControlId = messageControlId
    };

    public static Hl7ParseResult Failure(string reason) => new()
    {
        IsValid = false,
        ErrorReason = reason
    };
}