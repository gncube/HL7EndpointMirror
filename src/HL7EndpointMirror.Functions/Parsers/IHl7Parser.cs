using HL7EndpointMirror.Functions.Models;

namespace HL7EndpointMirror.Functions.Parsers;

public interface IHl7Parser
{
    Hl7ParseResult Parse(string message);
}