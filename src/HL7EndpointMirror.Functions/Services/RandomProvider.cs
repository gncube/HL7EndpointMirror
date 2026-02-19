namespace HL7EndpointMirror.Functions.Services;

public class RandomProvider : IRandomProvider
{
    private readonly Random _random = Random.Shared;

    public int Next(int minValue, int maxValue) =>
        _random.Next(minValue, maxValue);
}