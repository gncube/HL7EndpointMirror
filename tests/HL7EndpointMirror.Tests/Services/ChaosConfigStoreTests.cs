using FluentAssertions;

using HL7EndpointMirror.Functions.Models;
using HL7EndpointMirror.Functions.Services;

using Microsoft.Extensions.Options;

namespace HL7EndpointMirror.Tests.Services;

public class ChaosConfigStoreTests
{
    private static InMemoryChaosConfigStore CreateSut(ChaosConfig? seed = null)
    {
        var config = seed ?? new ChaosConfig();
        return new InMemoryChaosConfigStore(Options.Create(config));
    }

    [Fact]
    public async Task GetAsync_ReturnsSeededConfig()
    {
        var seed = new ChaosConfig { IsEnabled = true, FailureRatePercent = 42 };
        var sut = CreateSut(seed);

        var result = await sut.GetAsync();

        result.IsEnabled.Should().BeTrue();
        result.FailureRatePercent.Should().Be(42);
    }

    [Fact]
    public async Task SaveAsync_UpdatesStoredConfig()
    {
        var sut = CreateSut();
        var updated = new ChaosConfig { IsEnabled = true, FailureRatePercent = 75 };

        await sut.SaveAsync(updated);
        var result = await sut.GetAsync();

        result.IsEnabled.Should().BeTrue();
        result.FailureRatePercent.Should().Be(75);
    }

    [Fact]
    public async Task SaveAsync_ThenGet_ReturnsLatestValue()
    {
        var sut = CreateSut();

        await sut.SaveAsync(new ChaosConfig { FailureRatePercent = 10 });
        await sut.SaveAsync(new ChaosConfig { FailureRatePercent = 90 });

        var result = await sut.GetAsync();
        result.FailureRatePercent.Should().Be(90);
    }

    [Fact]
    public async Task GetAsync_BeforeSave_ReturnsDefaultConfig()
    {
        var sut = CreateSut();

        var result = await sut.GetAsync();

        result.IsEnabled.Should().BeFalse();
        result.FailureRatePercent.Should().Be(10);
    }
}