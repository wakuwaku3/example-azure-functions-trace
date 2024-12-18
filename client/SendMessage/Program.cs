﻿using Azure.Messaging.ServiceBus;
using Azure.Identity;
using Example.Azure.Functions.Trace.IAC.Contract;

// number of messages to be sent to the queue
const int numOfMessages = 10000;

// The Service Bus client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when messages are being published or read
// regularly.
//
// Set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
// If you use the default AmqpTcp, ensure that ports 5671 and 5672 are open.
var clientOptions = new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};

var client = new ServiceBusClient(
    ServiceBus.FullyQualifiedNamespace,
    new DefaultAzureCredential(),
    clientOptions);
try
{
    var tasks = new string[]
    {
        ServiceBus.ExampleQueue1Name,
        // ServiceBus.ExampleQueue2Name,
        // ServiceBus.ExampleQueue3Name,
    }.Select(async queue =>
    {
        // create a sender for the queue
        var sender = client.CreateSender(queue);

        try
        {
            // create a batch 
            var messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= numOfMessages; i++)
            {
                // try adding a message to the batch
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                {
                    await sender.SendMessagesAsync(messageBatch);
                    Console.WriteLine($"A batch of {i} messages has been published to the queue.");
                    messageBatch.Dispose();
                    messageBatch = await sender.CreateMessageBatchAsync();
                    messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}"));
                }
            }

            // Use the producer client to send the batch of messages to the Service Bus queue
            await sender.SendMessagesAsync(messageBatch);
            Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
            messageBatch.Dispose();
        }
        finally
        {
            // Calling DisposeAsync on client types is required to ensure that network
            // resources and other unmanaged objects are properly cleaned up.
            await sender.DisposeAsync();
        }
    });

    await Task.WhenAll(tasks);
}
finally
{
    await client.DisposeAsync();
}
