using FluentAssertions;

using HL7EndpointMirror.Functions.Models;
using HL7EndpointMirror.Functions.Services;

using Moq;

namespace HL7EndpointMirror.Tests.Services;

public class ChaosServiceTests
{
    private readonly Mock<IRandomProvider> _randomMock = new();

    private ChaosService CreateSut() => new(_randomMock.Object);

    // ── Chaos disabled ────────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_ChaosDisabled_AlwaysPasses()
    {
        var config = new ChaosConfig { IsEnabled = false };
        // Random should never be called when chaos is disabled
        _randomMock.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
                   .Returns(0); // Would always fail if chaos were active

        var result = await CreateSut().EvaluateAsync(config);

        result.ShouldFail.Should().BeFalse();
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task EvaluateAsync_ChaosDisabled_RandomNeverCalled()
    {
        var config = new ChaosConfig { IsEnabled = false };

        await CreateSut().EvaluateAsync(config);

        _randomMock.Verify(r => r.Next(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    // ── Chaos enabled — failure path ──────────────────────────────

    [Fact]
    public async Task EvaluateAsync_ChaosEnabled_RollBelowRate_Fails()
    {
        // 25% failure rate, roll = 10 → should fail
        var config = new ChaosConfig
        {
            IsEnabled = true,
            FailureRatePercent = 25,
            DefaultErrorType = "503"
        };
        _randomMock.Setup(r => r.Next(0, 100)).Returns(10);

        var result = await CreateSut().EvaluateAsync(config);

        result.ShouldFail.Should().BeTrue();
        result.StatusCode.Should().Be(503);
    }

    [Fact]
    public async Task EvaluateAsync_ChaosEnabled_RollEqualsRate_Passes()
    {
        // 25% failure rate, roll = 25 → boundary, should pass
        var config = new ChaosConfig
        {
            IsEnabled = true,
            FailureRatePercent = 25,
            DefaultErrorType = "503"
        };
        _randomMock.Setup(r => r.Next(0, 100)).Returns(25);

        var result = await CreateSut().EvaluateAsync(config);

        result.ShouldFail.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_ChaosEnabled_RollAboveRate_Passes()
    {
        // 25% failure rate, roll = 50 → should pass
        var config = new ChaosConfig
        {
            IsEnabled = true,
            FailureRatePercent = 25,
            DefaultErrorType = "503"
        };
        _randomMock.Setup(r => r.Next(0, 100)).Returns(50);

        var result = await CreateSut().EvaluateAsync(config);

        result.ShouldFail.Should().BeFalse();
        result.StatusCode.Should().Be(200);
    }

    // ── Failure rate boundaries ───────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_FailureRate100_AlwaysFails()
    {
        var config = new ChaosConfig
        {
            IsEnabled = true,
            FailureRatePercent = 100,
            DefaultErrorType = "500"
        };
        // Any roll 0–99 is < 100, always fails
        _randomMock.Setup(r => r.Next(0, 100)).Returns(99);

        var result = await CreateSut().EvaluateAsync(config);

        result.ShouldFail.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_FailureRate0_NeverFails()
    {
        var config = new ChaosConfig
        {
            IsEnabled = true,
            FailureRatePercent = 0,
            DefaultErrorType = "500"
        };
        // Roll 0 is not < 0, never fails
        _randomMock.Setup(r => r.Next(0, 100)).Returns(0);

        var result = await CreateSut().EvaluateAsync(config);

        result.ShouldFail.Should().BeFalse();
    }

    // ── Status code parsing ───────────────────────────────────────

    [Theory]
    [InlineData("500", 500)]
    [InlineData("503", 503)]
    [InlineData("429", 429)]
    public async Task EvaluateAsync_KnownErrorTypes_ReturnsCorrectStatusCode(
        string errorType, int expectedCode)
    {
        var config = new ChaosConfig
        {
            IsEnabled = true,
            FailureRatePercent = 100,
            DefaultErrorType = errorType
        };
        _randomMock.Setup(r => r.Next(0, 100)).Returns(0);

        var result = await CreateSut().EvaluateAsync(config);

        result.StatusCode.Should().Be(expectedCode);
    }

    [Fact]
    public async Task EvaluateAsync_InvalidErrorType_DefaultsTo500()
    {
        var config = new ChaosConfig
        {
            IsEnabled = true,
            FailureRatePercent = 100,
            DefaultErrorType = "not-a-number"
        };
        _randomMock.Setup(r => r.Next(0, 100)).Returns(0);

        var result = await CreateSut().EvaluateAsync(config);

        result.StatusCode.Should().Be(500);
    }

    // ── Latency simulation ────────────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_LatencyEnabled_ReturnsConfiguredDelay()
    {
        var config = new ChaosConfig
        {
            IsEnabled = false,
            LatencySimulation = new() { IsEnabled = true, DelayMs = 2000 }
        };

        var result = await CreateSut().EvaluateAsync(config);

        result.DelayMs.Should().Be(2000);
    }

    [Fact]
    public async Task EvaluateAsync_LatencyDisabled_ReturnsZeroDelay()
    {
        var config = new ChaosConfig
        {
            IsEnabled = false,
            LatencySimulation = new() { IsEnabled = false, DelayMs = 2000 }
        };

        var result = await CreateSut().EvaluateAsync(config);

        result.DelayMs.Should().Be(0);
    }

    [Fact]
    public async Task EvaluateAsync_ChaosFailWithLatency_ReturnsBothFailAndDelay()
    {
        var config = new ChaosConfig
        {
            IsEnabled = true,
            FailureRatePercent = 100,
            DefaultErrorType = "503",
            LatencySimulation = new() { IsEnabled = true, DelayMs = 1000 }
        };
        _randomMock.Setup(r => r.Next(0, 100)).Returns(0);

        var result = await CreateSut().EvaluateAsync(config);

        result.ShouldFail.Should().BeTrue();
        result.StatusCode.Should().Be(503);
        result.DelayMs.Should().Be(1000);
    }
}