using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Example.Azure.Functions.Trace.IAC.Contract;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Example.Azure.Functions.Trace.Function;

public class Function(ILogger<Function> logger)
{
    private readonly ILogger<Function> _logger = logger;

    [Function(nameof(Execute1))]
    [ServiceBusOutput(ServiceBus.ExampleQueue2Name)]
    public string Execute1(
    [ServiceBusTrigger(ServiceBus.ExampleQueue1Name)] ServiceBusReceivedMessage message)
    {
        _logger.LogInformation("Execute1 processed message");
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        _logger.LogDebug("debug log");
        _logger.LogWarning("warning log");
        _logger.LogError("error log");
        _logger.LogCritical("critical log");
        _logger.LogTrace("trace log");
        _logger.LogMetric("metric log", 1);

        var outputMessage = $"Output message created at {DateTime.Now}";
        return outputMessage;
    }

    [Function(nameof(Execute2))]
    [ServiceBusOutput(ServiceBus.ExampleQueue3Name)]
    public string Execute2(
    [ServiceBusTrigger(ServiceBus.ExampleQueue2Name)] ServiceBusReceivedMessage message)
    {
        _logger.LogInformation("Execute2 processed message");
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        throw new Exception("An error occurred in Execute2");
    }
}
