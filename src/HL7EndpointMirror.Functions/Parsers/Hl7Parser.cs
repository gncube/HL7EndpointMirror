using HL7EndpointMirror.Functions.Models;

namespace HL7EndpointMirror.Functions.Parsers;

public class Hl7Parser : IHl7Parser
{
    // MSH is always the first segment.
    // Fields are pipe-delimited. MSH.1 = '|' (field separator itself),
    // so MSH.10 (MessageControlId) is index 9 in the split array.
    private const int MessageControlIdIndex = 9;
    private const string MshPrefix = "MSH";

    public Hl7ParseResult Parse(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Hl7ParseResult.Failure("Message body is empty.");

        // Normalise line endings — HL7 segments separated by \r, \n, or \r\n
        var firstSegment = message
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        if (firstSegment is null || !firstSegment.StartsWith(MshPrefix, StringComparison.OrdinalIgnoreCase))
            return Hl7ParseResult.Failure("Message does not begin with an MSH segment.");

        var fields = firstSegment.Split('|');

        if (fields.Length <= MessageControlIdIndex)
            return Hl7ParseResult.Failure(
                $"MSH segment has only {fields.Length} fields; MessageControlId (MSH.10) not present.");

        var messageControlId = fields[MessageControlIdIndex].Trim();

        if (string.IsNullOrEmpty(messageControlId))
            return Hl7ParseResult.Failure("MSH.10 (MessageControlId) is empty.");

        return Hl7ParseResult.Success(messageControlId);
    }
}