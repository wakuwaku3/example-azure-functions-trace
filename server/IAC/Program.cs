using Pulumi;
using Pulumi.AzureNative.ServiceBus;
using Pulumi.AzureNative.Resources;
using System.Threading.Tasks;
using System.Linq;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.OperationalInsights;

namespace Example.Azure.Functions.Trace.IAC;
class Program
{
    static Task<int> Main() => Pulumi.Deployment.RunAsync<MyStack>();
}

public class MyStack : Stack
{
    public MyStack()
    {
        var resourceGroup = new ResourceGroup(Contract.ResourceGroup.Name, new ResourceGroupArgs
        {
            Location = Contract.ResourceGroup.Location,
            ResourceGroupName = Contract.ResourceGroup.Name,
        });

        // Service Bus Namespace の作成
        var serviceBusNamespace = new Namespace(Contract.ServiceBus.Namespace, new NamespaceArgs
        {
            NamespaceName = Contract.ServiceBus.Namespace,
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Sku = new Pulumi.AzureNative.ServiceBus.Inputs.SBSkuArgs
            {
                Name = SkuName.Basic,
                Tier = SkuTier.Basic,
            },
        });

        // Service Bus Queue の作成
        var queues = Contract.ServiceBus.Queues.Select(queue =>
            new Queue(queue.Name, new QueueArgs
            {
                ResourceGroupName = resourceGroup.Name,
                NamespaceName = serviceBusNamespace.Name,
                QueueName = queue.Name,
            })
        ).ToArray();

        // Create an Authorization Rule for the Namespace
        var authorizationRule = new NamespaceAuthorizationRule(Contract.ServiceBus.NamespaceAuthRule, new NamespaceAuthorizationRuleArgs
        {
            ResourceGroupName = resourceGroup.Name,
            NamespaceName = serviceBusNamespace.Name,
            Rights =
            {
                "Listen",
                "Send",
                "Manage"
            },
        });

        // Get the connection string for the authorization rule
        var primaryConnectionString = Output.Tuple(resourceGroup.Name, serviceBusNamespace.Name, authorizationRule.Name)
            .Apply(items => ListNamespaceKeys.InvokeAsync(new ListNamespaceKeysArgs
            {
                ResourceGroupName = items.Item1,
                NamespaceName = items.Item2,
                AuthorizationRuleName = items.Item3,
            })).Apply(keys => keys.PrimaryConnectionString);

        // Export the connection string as an output
        ServiceBusConnectionString = Output.CreateSecret(primaryConnectionString);

        // Log Analytics の作成
        var workspace = new Workspace(Contract.ApplicationInsights.WorkspaceName, new WorkspaceArgs
        {
            ResourceGroupName = resourceGroup.Name,
            WorkspaceName = Contract.ApplicationInsights.WorkspaceName,
        });

        // Application Insights の作成
        var component = new Component(Contract.ApplicationInsights.Name, new()
        {
            ApplicationType = ApplicationType.Other,
            Kind = "other",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            ResourceName = Contract.ApplicationInsights.Name,
            WorkspaceResourceId = workspace.Id,
        });

        ApplicationInsightsConnectionString = Output.CreateSecret(component.ConnectionString);
    }

    [Output("serviceBusConnectionString")]
    public Output<string> ServiceBusConnectionString { get; init; }
    [Output("applicationInsightsConnectionString")] public Output<string> ApplicationInsightsConnectionString { get; init; }
}
