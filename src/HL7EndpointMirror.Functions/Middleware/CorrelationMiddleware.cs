using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace HL7EndpointMirror.Functions.Middleware;

public class CorrelationMiddleware : IFunctionsWorkerMiddleware
{
    private const string MessageIdHeader = "X-Message-Id";

    private readonly ILogger<CorrelationMiddleware> _logger;

    public CorrelationMiddleware(ILogger<CorrelationMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var messageId = ExtractOrGenerateMessageId(context);

        // Store in FunctionContext.Items so any function can retrieve it
        context.Items[MessageIdHeader] = messageId;

        // Add to logging scope so every log line in this request includes MessageId
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["MessageId"] = messageId
        }))
        {
            await next(context);
        }
    }

    private static string ExtractOrGenerateMessageId(FunctionContext context)
    {
        // Try to get the HTTP request data from the context
        var requestData = context.GetHttpContext()?.Request;

        if (requestData is not null &&
            requestData.Headers.TryGetValue(MessageIdHeader, out var values))
        {
            var headerValue = values.ToString().Trim();
            if (!string.IsNullOrEmpty(headerValue))
                return headerValue;
        }

        // Generate a new ID if header is absent or empty
        return Guid.NewGuid().ToString();
    }
}