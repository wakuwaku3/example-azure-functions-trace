namespace Example.Azure.Functions.Trace.IAC.Contract;

public static class ApplicationInsights
{
    public static string Name { get; } = Prefix.With("appi");
    public static string WorkspaceName { get; } = Prefix.With("logs");
}
