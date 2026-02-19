using FluentAssertions;

using HL7EndpointMirror.Functions.Parsers;

namespace HL7EndpointMirror.Tests.Parsers;

public class Hl7ParserTests
{
    private readonly Hl7Parser _sut = new();

    // ── Happy path ────────────────────────────────────────────────

    [Fact]
    public void Parse_ValidMessage_ReturnsSuccess()
    {
        var message = "MSH|^~\\&|GLS|LAB|MIRROR|TEST|20260219120000||ORM^O01|MSG001|P|2.4\rPID|1||PAT001";

        var result = _sut.Parse(message);

        result.IsValid.Should().BeTrue();
        result.MessageControlId.Should().Be("MSG001");
        result.ErrorReason.Should().BeNull();
    }

    [Fact]
    public void Parse_MessageWithWindowsLineEndings_ReturnsSuccess()
    {
        var message = "MSH|^~\\&|GLS|LAB|MIRROR|TEST|20260219120000||ORM^O01|MSG002|P|2.4\r\nPID|1||PAT001";

        var result = _sut.Parse(message);

        result.IsValid.Should().BeTrue();
        result.MessageControlId.Should().Be("MSG002");
    }

    [Fact]
    public void Parse_MessageWithUnixLineEndings_ReturnsSuccess()
    {
        var message = "MSH|^~\\&|GLS|LAB|MIRROR|TEST|20260219120000||ORM^O01|MSG003|P|2.4\nPID|1||PAT001";

        var result = _sut.Parse(message);

        result.IsValid.Should().BeTrue();
        result.MessageControlId.Should().Be("MSG003");
    }

    [Fact]
    public void Parse_MessageControlIdWithWhitespace_ReturnsTrimmedValue()
    {
        var message = "MSH|^~\\&|GLS|LAB|MIRROR|TEST|20260219120000||ORM^O01| MSG004 |P|2.4";

        var result = _sut.Parse(message);

        result.IsValid.Should().BeTrue();
        result.MessageControlId.Should().Be("MSG004");
    }

    // ── Failure cases ─────────────────────────────────────────────

    [Fact]
    public void Parse_NullMessage_ReturnsFailure()
    {
        var result = _sut.Parse(null!);

        result.IsValid.Should().BeFalse();
        result.ErrorReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Parse_EmptyMessage_ReturnsFailure()
    {
        var result = _sut.Parse(string.Empty);

        result.IsValid.Should().BeFalse();
        result.ErrorReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Parse_WhitespaceOnlyMessage_ReturnsFailure()
    {
        var result = _sut.Parse("   ");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Parse_MessageNotStartingWithMsh_ReturnsFailure()
    {
        var message = "PID|1||PAT001^^^LAB^MR";

        var result = _sut.Parse(message);

        result.IsValid.Should().BeFalse();
        result.ErrorReason.Should().Contain("MSH");
    }

    [Fact]
    public void Parse_MshSegmentTooShort_ReturnsFailure()
    {
        // Only 5 fields — MSH.10 not reachable
        var message = "MSH|^~\\&|GLS|LAB|MIRROR";

        var result = _sut.Parse(message);

        result.IsValid.Should().BeFalse();
        result.ErrorReason.Should().Contain("MSH.10");
    }

    [Fact]
    public void Parse_EmptyMessageControlId_ReturnsFailure()
    {
        // MSH.10 is present but empty (two adjacent pipes at index 9)
        var message = "MSH|^~\\&|GLS|LAB|MIRROR|TEST|20260219120000||ORM^O01||P|2.4";

        var result = _sut.Parse(message);

        result.IsValid.Should().BeFalse();
        result.ErrorReason.Should().Contain("MSH.10");
    }

    // ── Theory: various valid MessageControlId formats ────────────

    [Theory]
    [InlineData("MSH|^~\\&|A|B|C|D|20260101||ADT^A01|ALPHA123|P|2.4", "ALPHA123")]
    [InlineData("MSH|^~\\&|A|B|C|D|20260101||ADT^A01|12345678|P|2.4", "12345678")]
    [InlineData("MSH|^~\\&|A|B|C|D|20260101||ADT^A01|MSG-001-XYZ|P|2.4", "MSG-001-XYZ")]
    public void Parse_VariousValidControlIds_ExtractsCorrectly(string message, string expectedId)
    {
        var result = _sut.Parse(message);

        result.IsValid.Should().BeTrue();
        result.MessageControlId.Should().Be(expectedId);
    }
}