using System.Net;
using System.Text.Json;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace HL7EndpointMirror.Functions.Functions;

public class HealthFunction
{
    private readonly ILogger<HealthFunction> _logger;

    public HealthFunction(ILogger<HealthFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(HealthFunction))]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/health")]
        HttpRequestData req,
        FunctionContext context)
    {
        _logger.LogInformation("Health check requested.");

        var payload = new
        {
            status = "healthy",
            timestamp = DateTimeOffset.UtcNow,
            version = GetVersion()
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        await response.WriteStringAsync(
            JsonSerializer.Serialize(payload, JsonOptions));

        return response;
    }

    // ── Private helpers ───────────────────────────────────────────

    private static string GetVersion() =>
        typeof(HealthFunction).Assembly
            .GetName()
            .Version?
            .ToString(3) ?? "0.0.0";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}