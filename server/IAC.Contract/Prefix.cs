namespace Example.Azure.Functions.Trace.IAC.Contract;

internal static class Prefix
{
    private static readonly string Value = Environment.GetEnvironmentVariable("AZURE_RESOURCE_PREFIX")?.Trim() ?? "example";

    internal static string With(string suffix) => string.IsNullOrWhiteSpace(Value) ? suffix : $"{Value}-{suffix}";
}
