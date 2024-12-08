using Microsoft.VisualBasic;

namespace Example.Azure.Functions.Trace.IAC.Contract;

public static class StorageAccount
{
    public static readonly string Name = Prefix.With("sa");
    public static readonly string AccountName = Strings.Replace(Name, "-", "", 1, -1, CompareMethod.Binary) ?? string.Empty;
}
