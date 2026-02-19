using Microsoft.Azure.Functions.Worker;

namespace HL7EndpointMirror.Functions.Middleware;

public static class FunctionContextExtensions
{
    private const string MessageIdHeader = "X-Message-Id";

    public static string GetMessageId(this FunctionContext context)
    {
        if (context.Items.TryGetValue(MessageIdHeader, out var value) &&
            value is string messageId &&
            !string.IsNullOrEmpty(messageId))
        {
            return messageId;
        }

        // Fallback — should not happen if middleware is registered correctly
        return Guid.NewGuid().ToString();
    }
}