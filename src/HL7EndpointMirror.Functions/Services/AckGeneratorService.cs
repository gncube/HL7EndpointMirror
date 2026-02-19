namespace HL7EndpointMirror.Functions.Services;

public class AckGeneratorService : IAckGeneratorService
{
    // HL7 ACK codes:
    //   AA = Application Accept
    //   AE = Application Error
    //   AR = Application Reject
    private const string SendingApplication = "MIRROR";
    private const string SendingFacility = "TEST";
    private const string ReceivingApplication = "GLS";
    private const string ReceivingFacility = "LAB";
    private const string ProcessingId = "P";
    private const string Version = "2.4";

    public string GenerateAck(string messageControlId, string ackCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageControlId);
        ArgumentException.ThrowIfNullOrWhiteSpace(ackCode);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var ackControlId = $"ACK{timestamp}";

        var ackMessage = GetAckMessage(ackCode);

        // HL7 segments are separated by carriage return (\r)
        return string.Join("\r",
            $"MSH|^~\\&|{SendingApplication}|{SendingFacility}|{ReceivingApplication}|{ReceivingFacility}|{timestamp}||ACK|{ackControlId}|{ProcessingId}|{Version}",
            $"MSA|{ackCode}|{messageControlId}|{ackMessage}"
        );
    }

    private static string GetAckMessage(string ackCode) => ackCode switch
    {
        "AA" => "Message accepted",
        "AE" => "Application error",
        "AR" => "Application reject",
        _ => "Unknown"
    };
}