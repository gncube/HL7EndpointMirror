using FluentAssertions;

using HL7EndpointMirror.Functions.Functions;

using Microsoft.Extensions.Logging.Abstractions;

namespace HL7EndpointMirror.Tests.Functions;

public class HealthFunctionTests
{
    [Fact]
    public void HealthFunction_CanBeConstructed()
    {
        var sut = new HealthFunction(NullLogger<HealthFunction>.Instance);
        sut.Should().NotBeNull();
    }
}