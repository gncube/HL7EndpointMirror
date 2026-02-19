using HL7EndpointMirror.Functions.Middleware;
using HL7EndpointMirror.Functions.Models;
using HL7EndpointMirror.Functions.Parsers;
using HL7EndpointMirror.Functions.Services;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Middleware registration
builder.UseMiddleware<CorrelationMiddleware>();

// Telemetry configuration
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Chaos config — bound from "ChaosMode" section, validated on startup
builder.Services
    .AddOptions<ChaosConfig>()
    .Bind(builder.Configuration.GetSection(ChaosConfig.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Core services
builder.Services.AddSingleton<IHl7Parser, Hl7Parser>();
builder.Services.AddSingleton<IAckGeneratorService, AckGeneratorService>();
builder.Services.AddSingleton<IRandomProvider, RandomProvider>();
builder.Services.AddScoped<IChaosService, ChaosService>();
builder.Services.AddSingleton<IChaosConfigStore, InMemoryChaosConfigStore>();

builder.Build().Run();
