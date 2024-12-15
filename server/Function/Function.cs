using Azure.Messaging.ServiceBus;
using Example.Azure.Functions.Trace.IAC.Contract;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Example.Azure.Functions.Trace.Function;

public class Function(ILogger<Function> logger)
{
    [Function(nameof(Execute1Async))]
    public async Task Execute1Async(
    [ServiceBusTrigger(ServiceBus.ExampleQueue1Name, AutoCompleteMessages = true)] ServiceBusReceivedMessage message)
    {
        logger.LogInformation("{name} executing... id: {id}", nameof(Execute1Async), message.MessageId);
        await Task.Delay(100);
        logger.LogInformation("{name} executed. id: {id}", nameof(Execute1Async), message.MessageId);
    }

    [Function(nameof(Execute2Async))]
    public async Task Execute2Async(
    [ServiceBusTrigger(ServiceBus.ExampleQueue2Name, IsBatched = true, AutoCompleteMessages = true)] ServiceBusReceivedMessage[] messages)
    {
        logger.LogInformation($"{nameof(Execute2Async)} executing...");
        foreach (var message in messages)
        {
            logger.LogInformation("{name} executing... id: {id}", nameof(Execute2Async), message.MessageId);
            await Task.Delay(100);
            logger.LogInformation("{name} executed. id: {id}", nameof(Execute2Async), message.MessageId);
        }
    }

    private static readonly SemaphoreSlim semaphore = new(16);
    [Function(nameof(Execute3Async))]
    public async Task Execute3Async(
    [ServiceBusTrigger(ServiceBus.ExampleQueue3Name, IsBatched = true, AutoCompleteMessages = true)] ServiceBusReceivedMessage[] messages)
    {
        logger.LogInformation($"{nameof(Execute3Async)} executing...");

        var tasks = messages.Select(async message =>
        {
            await semaphore.WaitAsync();
            try
            {
                logger.LogInformation("{name} executing... id: {id}", nameof(Execute3Async), message.MessageId);
                await Task.Delay(100);
                logger.LogInformation("{name} executed. id: {id}", nameof(Execute3Async), message.MessageId);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }
}
