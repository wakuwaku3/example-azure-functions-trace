using Azure.Messaging.ServiceBus;
using Example.Azure.Functions.Trace.IAC.Contract;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Example.Azure.Functions.Trace.Function;

public class Function(ILogger<Function> logger)
{
    [Function(nameof(Execute1))]
    [ServiceBusOutput(ServiceBus.ExampleQueue2Name)]
    public string Execute1(
    [ServiceBusTrigger(ServiceBus.ExampleQueue1Name)] ServiceBusReceivedMessage message)
    {
        logger.LogInformation($"{nameof(Execute1)} executing...");
        var outputMessage = $"Output message created at {DateTime.Now}";
        return outputMessage;
    }

    [Function(nameof(Execute2))]
    [ServiceBusOutput(ServiceBus.ExampleQueue3Name)]
    public string Execute2(
    [ServiceBusTrigger(ServiceBus.ExampleQueue2Name, IsBatched = true)] ServiceBusReceivedMessage[] messages)
    {
        logger.LogInformation($"{nameof(Execute2)} executing...");
        foreach (var message in messages)
        {
            logger.LogInformation("MessageId: {id}", message.MessageId);
        }
        throw new Exception("An error occurred in Execute2");
    }
}
