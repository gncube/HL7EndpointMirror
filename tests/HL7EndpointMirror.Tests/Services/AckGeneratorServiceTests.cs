using FluentAssertions;

using HL7EndpointMirror.Functions.Services;

namespace HL7EndpointMirror.Tests.Services;

public class AckGeneratorServiceTests
{
    private readonly AckGeneratorService _sut = new();

    // ── Structure tests ───────────────────────────────────────────

    [Fact]
    public void GenerateAck_ValidInputs_ReturnsTwoSegments()
    {
        var result = _sut.GenerateAck("MSG001", "AA");

        var segments = result.Split('\r');
        segments.Should().HaveCount(2);
    }

    [Fact]
    public void GenerateAck_ValidInputs_FirstSegmentIsMsh()
    {
        var result = _sut.GenerateAck("MSG001", "AA");

        var msh = result.Split('\r')[0];
        msh.Should().StartWith("MSH|");
    }

    [Fact]
    public void GenerateAck_ValidInputs_SecondSegmentIsMsa()
    {
        var result = _sut.GenerateAck("MSG001", "AA");

        var msa = result.Split('\r')[1];
        msa.Should().StartWith("MSA|");
    }

    // ── MessageControlId echo tests ───────────────────────────────

    [Fact]
    public void GenerateAck_AaCode_MsaContainsOriginalMessageControlId()
    {
        var result = _sut.GenerateAck("MSG001", "AA");

        var msa = result.Split('\r')[1];
        var fields = msa.Split('|');

        // MSA.2 = original MessageControlId (index 2)
        fields[2].Should().Be("MSG001");
    }

    [Fact]
    public void GenerateAck_AeCode_MsaContainsOriginalMessageControlId()
    {
        var result = _sut.GenerateAck("ERR-MSG-42", "AE");

        var msa = result.Split('\r')[1];
        msa.Split('|')[2].Should().Be("ERR-MSG-42");
    }

    [Fact]
    public void GenerateAck_ArCode_MsaContainsOriginalMessageControlId()
    {
        var result = _sut.GenerateAck("REJECT-99", "AR");

        var msa = result.Split('\r')[1];
        msa.Split('|')[2].Should().Be("REJECT-99");
    }

    // ── ACK code tests ────────────────────────────────────────────

    [Theory]
    [InlineData("AA", "Message accepted")]
    [InlineData("AE", "Application error")]
    [InlineData("AR", "Application reject")]
    public void GenerateAck_KnownAckCodes_MsaContainsCorrectMessage(
        string ackCode, string expectedMessage)
    {
        var result = _sut.GenerateAck("MSG001", ackCode);

        var msa = result.Split('\r')[1];
        msa.Should().Contain(expectedMessage);
    }

    [Fact]
    public void GenerateAck_UnknownAckCode_MsaContainsUnknown()
    {
        var result = _sut.GenerateAck("MSG001", "ZZ");

        var msa = result.Split('\r')[1];
        msa.Should().Contain("Unknown");
    }

    // ── MSH field tests ───────────────────────────────────────────

    [Fact]
    public void GenerateAck_MshSegment_ContainsSendingApplication()
    {
        var result = _sut.GenerateAck("MSG001", "AA");

        var msh = result.Split('\r')[0];
        msh.Should().Contain("MIRROR");
    }

    [Fact]
    public void GenerateAck_MshSegment_MessageTypeIsAck()
    {
        var result = _sut.GenerateAck("MSG001", "AA");

        var msh = result.Split('\r')[0];
        var fields = msh.Split('|');

        // MSH.9 = message type (index 8)
        fields[8].Should().Be("ACK");
    }

    [Fact]
    public void GenerateAck_MshSegment_AckControlIdStartsWithAck()
    {
        var result = _sut.GenerateAck("MSG001", "AA");

        var msh = result.Split('\r')[0];
        var fields = msh.Split('|');

        // MSH.10 = ACK's own control id (index 9)
        fields[9].Should().StartWith("ACK");
    }

    // ── Guard clause tests ────────────────────────────────────────

    [Fact]
    public void GenerateAck_NullMessageControlId_ThrowsArgumentException()
    {
        var act = () => _sut.GenerateAck(null!, "AA");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateAck_EmptyMessageControlId_ThrowsArgumentException()
    {
        var act = () => _sut.GenerateAck(string.Empty, "AA");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateAck_NullAckCode_ThrowsArgumentException()
    {
        var act = () => _sut.GenerateAck("MSG001", null!);

        act.Should().Throw<ArgumentException>();
    }
}