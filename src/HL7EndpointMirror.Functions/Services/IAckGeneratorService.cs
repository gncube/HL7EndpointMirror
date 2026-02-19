namespace HL7EndpointMirror.Functions.Services;

public interface IAckGeneratorService
{
    string GenerateAck(string messageControlId, string ackCode);
}