namespace HL7EndpointMirror.Functions.Services;

public interface IRandomProvider
{
    int Next(int minValue, int maxValue);
}