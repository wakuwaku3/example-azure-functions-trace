namespace Example.Azure.Functions.Trace.IAC.Contract;

public static class ServiceBus
{
    public static string Namespace { get; } = Prefix.With("sbn");

    public static string FullyQualifiedNamespace { get; } = $"{Namespace}.servicebus.windows.net";

    public static string NamespaceAuthRule { get; } = "SharedAccessKey";

    public static IEnumerable<Queue> Queues =>
    [
        new Queue { Name = ExampleQueue1Name },
        new Queue { Name = ExampleQueue2Name },
        new Queue { Name = ExampleQueue3Name },
    ];

    public const string ExampleQueue1Name = "example-queue1";
    public const string ExampleQueue2Name = "example-queue2";
    public const string ExampleQueue3Name = "example-queue3";

    public record Queue
    {
        public required string Name { get; init; }
    }
}
