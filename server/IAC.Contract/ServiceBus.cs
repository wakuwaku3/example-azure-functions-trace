namespace Example.Azure.Functions.Trace.IAC.Contract;

public static class ServiceBus
{
    public static string Namespace { get; } = Prefix.With("sbn");

    public static string NamespaceAuthRule { get; } = "SharedAccessKey";

    public static IEnumerable<Queue> Queues =>
    [
        new Queue{Name = "example-queue"},
    ];

    public record Queue
    {
        public required string Name { get; init; }
    }
}
