Backend Architecture (.NET 10 Isolated Worker)
Project Structure
HL7EndpointMirror/
├── HL7EndpointMirror.Functions/ # Azure Functions project
│ ├── Functions/
│ │ ├── Hl7MessageFunction.cs # POST /api/v1/hl7/messages
│ │ ├── HealthFunction.cs # GET /api/v1/health
│ │ └── ChaosConfigFunction.cs # GET/PUT /api/v1/admin/chaos
│ ├── Services/
│ │ ├── IAckGeneratorService.cs
│ │ ├── AckGeneratorService.cs
│ │ ├── IChaosService.cs
│ │ └── ChaosService.cs
│ ├── Parsers/
│ │ ├── IHl7Parser.cs
│ │ └── Hl7Parser.cs
│ ├── Models/
│ │ ├── ChaosConfig.cs # IOptions<T> config model
│ │ ├── Hl7ParseResult.cs
│ │ └── RequestLogEntry.cs
│ ├── Middleware/
│ │ └── CorrelationMiddleware.cs # X-Message-Id propagation
│ ├── host.json
│ ├── local.settings.json # gitignored
│ └── Program.cs # DI composition root
│
└── HL7EndpointMirror.Tests/ # xUnit test project
├── Functions/
├── Services/
└── Parsers/
Dependency Injection Composition (Program.cs)
csharp// Pattern only — not production code
var host = new HostBuilder()
.ConfigureFunctionsWorkerDefaults(worker =>
{
worker.UseMiddleware<CorrelationMiddleware>();
})
.ConfigureServices((context, services) =>
{
services.AddApplicationInsightsTelemetryWorkerService();
services.AddOptions<ChaosConfig>()
.Bind(context.Configuration.GetSection("ChaosMode"))
.ValidateDataAnnotations()
.ValidateOnStart();
services.AddSingleton<IHl7Parser, Hl7Parser>();
services.AddSingleton<IAckGeneratorService, AckGeneratorService>();
services.AddScoped<IChaosService, ChaosService>();
})
.Build();
Request Processing Pipeline
HTTP Request
│
▼
CorrelationMiddleware
│ - Extract X-Message-Id (generate if absent)
│ - Set correlation context for logging
▼
Hl7MessageFunction.RunAsync()
│
├── Validate Content-Type == application/hl7-v2
│ └── 415 Unsupported Media Type if not
│
├── Read body as string
│ └── 400 Bad Request if empty
│
├── IHl7Parser.Parse(body)
│ └── Returns Hl7ParseResult { MessageControlId, IsValid, ErrorReason }
│ └── 400 if parse fails
│
├── IChaosService.EvaluateAsync(chaosConfig)
│ └── Returns ChaosDecision { ShouldFail, StatusCode, DelayMs }
│ └── If ShouldFail: log + return configured error immediately
│
├── Task.Delay(decision.DelayMs) // 0 if no latency simulation
│
├── IAckGeneratorService.GenerateAck(messageControlId, ackCode: "AA")
│
├── Log RequestLogEntry to ILogger (→ Application Insights)
│ └── NEVER log raw HL7 body
│
└── Return 200 with HL7 ACK body
ChaosConfig Model with Validation
csharppublic class ChaosConfig
{
[Required]
public bool IsEnabled { get; set; } = false;

    [Range(0, 100)]
    public int FailureRatePercent { get; set; } = 10;

    [Required]
    public string DefaultErrorType { get; set; } = "500";

    public LatencyConfig LatencySimulation { get; set; } = new();

}

public class LatencyConfig
{
public bool IsEnabled { get; set; } = false;

    [Range(0, 30000)]
    public int DelayMs { get; set; } = 0;

}
