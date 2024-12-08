using Pulumi;
using Pulumi.AzureNative.ServiceBus;
using Pulumi.AzureNative.Resources;
using System.Threading.Tasks;
using System.Linq;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.OperationalInsights;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;

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
        ResourceGroupName = resourceGroup.Name;

        // Service Bus Namespace の作成
        var serviceBusNamespace = new Namespace(Contract.ServiceBus.Namespace, new NamespaceArgs
        {
            NamespaceName = Contract.ServiceBus.Namespace,
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Sku = new Pulumi.AzureNative.ServiceBus.Inputs.SBSkuArgs
            {
                Name = Pulumi.AzureNative.ServiceBus.SkuName.Basic,
                Tier = SkuTier.Basic,
            },
        }, new CustomResourceOptions { DependsOn = { resourceGroup } });

        // Service Bus Queue の作成
        var queues = Contract.ServiceBus.Queues.Select(queue =>
            new Pulumi.AzureNative.ServiceBus.Queue(queue.Name, new Pulumi.AzureNative.ServiceBus.QueueArgs
            {
                ResourceGroupName = resourceGroup.Name,
                NamespaceName = serviceBusNamespace.Name,
                QueueName = queue.Name,
            }, new CustomResourceOptions { DependsOn = { resourceGroup, serviceBusNamespace } })
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
        }, new CustomResourceOptions { DependsOn = { resourceGroup, serviceBusNamespace } });

        // Get the connection string for the authorization rule
        var primaryConnectionString = Output.Tuple(resourceGroup.Name, serviceBusNamespace.Name, authorizationRule.Name)
            .Apply(items => ListNamespaceKeys.InvokeAsync(new ListNamespaceKeysArgs
            {
                ResourceGroupName = items.Item1,
                NamespaceName = items.Item2,
                AuthorizationRuleName = items.Item3,
            })).Apply(keys => keys.PrimaryConnectionString);

        // Export the connection string as an output
        var serviceBusConnectionString = Output.CreateSecret(primaryConnectionString);

        // Log Analytics の作成
        var workspace = new Workspace(Contract.ApplicationInsights.WorkspaceName, new WorkspaceArgs
        {
            ResourceGroupName = resourceGroup.Name,
            WorkspaceName = Contract.ApplicationInsights.WorkspaceName,
        }, new CustomResourceOptions { DependsOn = { resourceGroup } });

        // Application Insights の作成
        var component = new Component(Contract.ApplicationInsights.Name, new()
        {
            ApplicationType = ApplicationType.Other,
            Kind = "other",
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            ResourceName = Contract.ApplicationInsights.Name,
            WorkspaceResourceId = workspace.Id,
        }, new CustomResourceOptions { DependsOn = { resourceGroup } });

        var applicationInsightsConnectionString = Output.CreateSecret(component.ConnectionString);

        // storageAccount を作成
        var storageAccount = new StorageAccount(Contract.StorageAccount.Name, new StorageAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = Contract.StorageAccount.AccountName,
            Sku = new SkuArgs
            {
                Name = Pulumi.AzureNative.Storage.SkuName.Standard_LRS,
            },
            Kind = Pulumi.AzureNative.Storage.Kind.StorageV2
        }, new CustomResourceOptions { DependsOn = { resourceGroup } });
        var storageConnectionString = Output.Tuple(resourceGroup.Name, storageAccount.Name).Apply(async items => await ListStorageAccountKeys.InvokeAsync(new ListStorageAccountKeysArgs
        {
            ResourceGroupName = items.Item1,
            AccountName = items.Item2,
        })).Apply(keys =>
        {
            var key = keys.Keys.First();
            return Output.Format($"DefaultEndpointsProtocol=https;AccountName={storageAccount.Name};AccountKey={key.Value};EndpointSuffix=core.windows.net");
        });


        // Create an App Service Plan
        var appServicePlan = new AppServicePlan(Contract.AppServicePlan.Name, new AppServicePlanArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Name = Contract.AppServicePlan.Name,
            Kind = "Linux",
            Sku = new SkuDescriptionArgs
            {
                Name = "B1",
                Tier = "Basic",
            },
            Reserved = true,
        }, new CustomResourceOptions { DependsOn = { resourceGroup } });

        // Create a Function App
        var functionApp = new WebApp(Contract.FunctionApp.Name, new WebAppArgs
        {
            Name = Contract.FunctionApp.Name,
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = appServicePlan.Id,
            Kind = "functionapp,linux",
            Reserved = true,
            SiteConfig = new SiteConfigArgs
            {
                AppSettings =
                {
                    new NameValuePairArgs
                    {
                        Name = "AzureWebJobsStorage",
                        Value = storageConnectionString,
                    },
                    new NameValuePairArgs
                    {
                        Name = "FUNCTIONS_WORKER_RUNTIME",
                        Value = "dotnet-isolated",
                    },
                    new NameValuePairArgs
                    {
                        Name = "AzureWebJobsServiceBus",
                        Value = serviceBusConnectionString,
                    },
                    new NameValuePairArgs
                    {
                        Name = "APPLICATIONINSIGHTS_CONNECTION_STRING",
                        Value = applicationInsightsConnectionString,
                    },
                    new NameValuePairArgs
                    {
                        Name = "WEBSITE_RUN_FROM_PACKAGE",
                        Value = "1"
                    },
                    new NameValuePairArgs
                    {
                        Name = "FUNCTIONS_EXTENSION_VERSION",
                        Value = "~4",
                    },
                },
                LinuxFxVersion = "DOTNET-ISOLATED|8.0",
            },
        }, new CustomResourceOptions { DependsOn = { appServicePlan, storageAccount, serviceBusNamespace, component } });
        FunctionAppName = functionApp.Name;
    }
    [Output("resourceGroupName")] public Output<string> ResourceGroupName { get; init; }
    [Output("functionAppName")] public Output<string> FunctionAppName { get; init; }
}
