using System.Text.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Example.Azure.Functions.Trace.Function;

public class FunctionExecutionTrackingMiddleware(TelemetryClient telemetryClient) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var traceTelemetry = new TraceTelemetry($"FunctionExecution-{context.FunctionDefinition.Name}", SeverityLevel.Information);
        traceTelemetry.Properties.Add("FunctionName", context.FunctionDefinition.Name);
        traceTelemetry.Properties.Add("InvocationId", context.InvocationId);

        foreach (var binding in context.BindingContext.BindingData)
        {
            if (binding.Value is not null)
            {
                if (binding.Value is string stringValue)
                {
                    traceTelemetry.Properties.Add($"BindingData-{binding.Key}", stringValue);
                }
                else
                {
                    traceTelemetry.Properties.Add($"BindingData-{binding.Key}", JsonSerializer.Serialize(binding.Value));
                }
            }
            else
            {
                traceTelemetry.Properties.Add($"BindingData-{binding.Key}", "null");
            }
        }

        telemetryClient.TrackTrace(traceTelemetry);

        await next(context);
    }
}
