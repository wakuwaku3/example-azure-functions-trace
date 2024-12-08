using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example.Azure.Functions.Trace.IAC.Contract;

public static class ResourceGroup
{
    public static string Name { get; } = Prefix.With("rg");
    public static string Location { get; } = "Japan East";
}
