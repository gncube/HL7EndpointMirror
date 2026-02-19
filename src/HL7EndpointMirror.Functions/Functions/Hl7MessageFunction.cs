using System.Diagnostics;
using System.Net;

using HL7EndpointMirror.Functions.Middleware;
using HL7EndpointMirror.Functions.Models;
using HL7EndpointMirror.Functions.Parsers;
using HL7EndpointMirror.Functions.Services;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HL7EndpointMirror.Functions.Functions;

public class Hl7MessageFunction
{
    private const string Hl7ContentType = "application/hl7-v2";

    private readonly IHl7Parser _parser;
    private readonly IAckGeneratorService _ackGenerator;
    private readonly IChaosService _chaosService;
    private readonly IOptions<ChaosConfig> _chaosConfig;
    private readonly ILogger<Hl7MessageFunction> _logger;

    public Hl7MessageFunction(
        IHl7Parser parser,
        IAckGeneratorService ackGenerator,
        IChaosService chaosService,
        IOptions<ChaosConfig> chaosConfig,
        ILogger<Hl7MessageFunction> logger)
    {
        _parser = parser;
        _ackGenerator = ackGenerator;
        _chaosService = chaosService;
        _chaosConfig = chaosConfig;
        _logger = logger;
    }

    [Function(nameof(Hl7MessageFunction))]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/hl7/messages")]
        HttpRequestData req,
        FunctionContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var messageId = context.GetMessageId();

        // ── 1. Validate Content-Type ──────────────────────────────
        if (!IsHl7ContentType(req))
        {
            _logger.LogWarning("Invalid Content-Type. MessageId: {MessageId}", messageId);
            return await BuildErrorResponseAsync(req, HttpStatusCode.UnsupportedMediaType,
                "Expected Content-Type: application/hl7-v2");
        }

        // ── 2. Read body ──────────────────────────────────────────
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
        {
            _logger.LogWarning("Empty request body. MessageId: {MessageId}", messageId);
            return await BuildErrorResponseAsync(req, HttpStatusCode.BadRequest,
                "Request body is empty.");
        }

        // ── 3. Parse HL7 ──────────────────────────────────────────
        var parseResult = _parser.Parse(body);

        if (!parseResult.IsValid)
        {
            _logger.LogWarning(
                "HL7 parse failure. MessageId: {MessageId} Reason: {Reason}",
                messageId, parseResult.ErrorReason);

            var rejectAck = _ackGenerator.GenerateAck("UNKNOWN", "AR");
            stopwatch.Stop();

            await LogEntryAsync(messageId, "UNKNOWN", 400, stopwatch.ElapsedMilliseconds,
                chaosApplied: false, chaosType: null, ackCode: "AR");

            return await BuildHl7ResponseAsync(req, HttpStatusCode.BadRequest, rejectAck);
        }

        var messageControlId = parseResult.MessageControlId;

        // ── 4. Evaluate chaos ─────────────────────────────────────
        var decision = await _chaosService.EvaluateAsync(_chaosConfig.Value);

        // ── 5. Apply latency simulation ───────────────────────────
        if (decision.DelayMs > 0)
        {
            _logger.LogInformation(
                "Latency simulation: delaying {DelayMs}ms. MessageId: {MessageId}",
                decision.DelayMs, messageId);

            await Task.Delay(decision.DelayMs);
        }

        // ── 6. Apply chaos failure ────────────────────────────────
        if (decision.ShouldFail)
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "Chaos failure applied. MessageId: {MessageId} StatusCode: {StatusCode}",
                messageId, decision.StatusCode);

            await LogEntryAsync(messageId, messageControlId, decision.StatusCode,
                stopwatch.ElapsedMilliseconds, chaosApplied: true,
                chaosType: _chaosConfig.Value.DefaultErrorType, ackCode: "CHAOS");

            var chaosResponse = req.CreateResponse((HttpStatusCode)decision.StatusCode);
            return chaosResponse;
        }

        // ── 7. Generate ACK and respond ───────────────────────────
        var ack = _ackGenerator.GenerateAck(messageControlId, "AA");
        stopwatch.Stop();

        await LogEntryAsync(messageId, messageControlId, 200,
            stopwatch.ElapsedMilliseconds, chaosApplied: false,
            chaosType: null, ackCode: "AA");

        _logger.LogInformation(
            "Message accepted. MessageId: {MessageId} MessageControlId: {MessageControlId} " +
            "ProcessingTime: {ProcessingTimeMs}ms",
            messageId, messageControlId, stopwatch.ElapsedMilliseconds);

        return await BuildHl7ResponseAsync(req, HttpStatusCode.OK, ack);
    }

    // ── Private helpers ───────────────────────────────────────────

    private static bool IsHl7ContentType(HttpRequestData req)
    {
        if (!req.Headers.TryGetValues("Content-Type", out var values))
            return false;

        return values.Any(v =>
            v.StartsWith(Hl7ContentType, StringComparison.OrdinalIgnoreCase));
    }

    private Task LogEntryAsync(
        string messageId,
        string messageControlId,
        int statusCode,
        long processingTimeMs,
        bool chaosApplied,
        string? chaosType,
        string ackCode)
    {
        // Structured log — picked up by Application Insights as custom dimensions
        // NOTE: raw HL7 body is intentionally never logged (HIPAA requirement)
        _logger.LogInformation(
            "RequestLog | MessageId: {MessageId} | MessageControlId: {MessageControlId} | " +
            "StatusCode: {StatusCode} | ProcessingTimeMs: {ProcessingTimeMs} | " +
            "ChaosApplied: {ChaosApplied} | ChaosType: {ChaosType} | AckCode: {AckCode}",
            messageId, messageControlId, statusCode, processingTimeMs,
            chaosApplied, chaosType ?? "none", ackCode);

        return Task.CompletedTask;
    }

    private static async Task<HttpResponseData> BuildHl7ResponseAsync(
        HttpRequestData req,
        HttpStatusCode statusCode,
        string body)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", Hl7ContentType);
        await response.WriteStringAsync(body);
        return response;
    }

    private static async Task<HttpResponseData> BuildErrorResponseAsync(
        HttpRequestData req,
        HttpStatusCode statusCode,
        string message)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync($"{{\"error\":\"{message}\"}}");
        return response;
    }
}