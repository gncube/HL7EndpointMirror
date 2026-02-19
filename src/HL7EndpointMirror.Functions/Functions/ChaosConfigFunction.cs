using System.Net;
using System.Text.Json;

using HL7EndpointMirror.Functions.Models;
using HL7EndpointMirror.Functions.Services;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace HL7EndpointMirror.Functions.Functions;

public class ChaosConfigFunction
{
    private readonly IChaosConfigStore _store;
    private readonly ILogger<ChaosConfigFunction> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public ChaosConfigFunction(
        IChaosConfigStore store,
        ILogger<ChaosConfigFunction> logger)
    {
        _store = store;
        _logger = logger;
    }

    // ── GET /api/v1/admin/chaos ───────────────────────────────────

    [Function(nameof(GetChaosConfig))]
    public async Task<HttpResponseData> GetChaosConfig(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/admin/chaos")]
        HttpRequestData req,
        FunctionContext context)
    {
        _logger.LogInformation("Chaos config requested.");

        var config = await _store.GetAsync();
        return await BuildJsonResponseAsync(req, HttpStatusCode.OK, config);
    }

    // ── PUT /api/v1/admin/chaos ───────────────────────────────────

    [Function(nameof(UpdateChaosConfig))]
    public async Task<HttpResponseData> UpdateChaosConfig(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/admin/chaos")]
        HttpRequestData req,
        FunctionContext context)
    {
        // ── 1. Read and deserialise body ──────────────────────────
        var body = await new StreamReader(req.Body).ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
            return await BuildErrorResponseAsync(req, HttpStatusCode.BadRequest,
                "Request body is empty.");

        ChaosConfig? incoming;

        try
        {
            incoming = JsonSerializer.Deserialize<ChaosConfig>(body, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Invalid JSON in chaos config update: {Message}", ex.Message);
            return await BuildErrorResponseAsync(req, HttpStatusCode.BadRequest,
                "Request body is not valid JSON.");
        }

        if (incoming is null)
            return await BuildErrorResponseAsync(req, HttpStatusCode.BadRequest,
                "Request body deserialised to null.");

        // ── 2. Validate ───────────────────────────────────────────
        var validationErrors = Validate(incoming);

        if (validationErrors.Count > 0)
        {
            var errorPayload = new
            {
                error = "Validation failed",
                details = validationErrors
            };

            return await BuildJsonResponseAsync(req, HttpStatusCode.BadRequest, errorPayload);
        }

        // ── 3. Persist ────────────────────────────────────────────
        await _store.SaveAsync(incoming);

        _logger.LogInformation(
            "Chaos config updated. IsEnabled: {IsEnabled} FailureRate: {FailureRate}% ErrorType: {ErrorType}",
            incoming.IsEnabled, incoming.FailureRatePercent, incoming.DefaultErrorType);

        return await BuildJsonResponseAsync(req, HttpStatusCode.OK, incoming);
    }

    // ── Private helpers ───────────────────────────────────────────

    private static List<string> Validate(ChaosConfig config)
    {
        var errors = new List<string>();

        if (config.FailureRatePercent < 0 || config.FailureRatePercent > 100)
            errors.Add("FailureRatePercent must be between 0 and 100.");

        if (string.IsNullOrWhiteSpace(config.DefaultErrorType))
            errors.Add("DefaultErrorType is required.");

        if (config.LatencySimulation.DelayMs < 0 || config.LatencySimulation.DelayMs > 30000)
            errors.Add("LatencySimulation.DelayMs must be between 0 and 30000.");

        return errors;
    }

    private static async Task<HttpResponseData> BuildJsonResponseAsync<T>(
        HttpRequestData req,
        HttpStatusCode statusCode,
        T payload)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(payload, JsonOptions));
        return response;
    }

    private static async Task<HttpResponseData> BuildErrorResponseAsync(
        HttpRequestData req,
        HttpStatusCode statusCode,
        string message)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(
            JsonSerializer.Serialize(new { error = message }, JsonOptions));
        return response;
    }
}