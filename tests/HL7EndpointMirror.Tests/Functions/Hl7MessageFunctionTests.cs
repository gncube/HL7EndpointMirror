using FluentAssertions;

using HL7EndpointMirror.Functions.Functions;
using HL7EndpointMirror.Functions.Models;
using HL7EndpointMirror.Functions.Parsers;
using HL7EndpointMirror.Functions.Services;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HL7EndpointMirror.Tests.Functions;

public class Hl7MessageFunctionTests
{
    private readonly Mock<IHl7Parser> _parserMock = new();
    private readonly Mock<IAckGeneratorService> _ackGeneratorMock = new();
    private readonly Mock<IChaosService> _chaosServiceMock = new();

    private readonly IOptions<ChaosConfig> _chaosOptions =
        Options.Create(new ChaosConfig { IsEnabled = false });

    private Hl7MessageFunction CreateSut() => new(
        _parserMock.Object,
        _ackGeneratorMock.Object,
        _chaosServiceMock.Object,
        _chaosOptions,
        NullLogger<Hl7MessageFunction>.Instance);

    [Fact]
    public void Hl7MessageFunction_CanBeConstructed()
    {
        // Verifies all dependencies resolve without error
        var sut = CreateSut();
        sut.Should().NotBeNull();
    }

    [Fact]
    public void ChaosConfig_DefaultValues_AreCorrect()
    {
        var config = new ChaosConfig();

        config.IsEnabled.Should().BeFalse();
        config.FailureRatePercent.Should().Be(10);
        config.DefaultErrorType.Should().Be("500");
        config.LatencySimulation.IsEnabled.Should().BeFalse();
        config.LatencySimulation.DelayMs.Should().Be(0);
    }

    [Fact]
    public void ChaosConfig_InvalidFailureRate_FailsValidation()
    {
        var config = new ChaosConfig { FailureRatePercent = 150 };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            config,
            new ValidationContext(config),
            validationResults,
            validateAllProperties: true);

        isValid.Should().BeFalse();
        validationResults.Should().ContainSingle(r =>
            r.MemberNames.Contains(nameof(ChaosConfig.FailureRatePercent)));
    }

    [Fact]
    public void ChaosConfig_ValidFailureRate_PassesValidation()
    {
        var config = new ChaosConfig { FailureRatePercent = 50 };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            config,
            new ValidationContext(config),
            validationResults,
            validateAllProperties: true);

        isValid.Should().BeTrue();
    }
}