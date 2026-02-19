using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HL7EndpointMirror.Functions;

public class Hl7MessageFunction
{
    private readonly ILogger<Hl7MessageFunction> _logger;

    public Hl7MessageFunction(ILogger<Hl7MessageFunction> logger)
    {
        _logger = logger;
    }

    [Function("Hl7MessageFunction")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}