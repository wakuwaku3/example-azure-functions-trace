#!/bin/bash
script_path=$(readlink -f "$0")
script_dir=$(dirname "$script_path")

cd $script_dir
root_path=$(builtin cd $script_dir/..; pwd)
cd $root_path

cd server/IAC

stack_name=dev

# stack を作る（初回以降はエラーなるので結果は破棄する）
pulumi stack init $stack_name 2>/dev/null

pulumi up -s -y $stack_name
service_bus_connection_string=$(pulumi stack output -s $stack_name --show-secrets serviceBusConnectionString)
application_insights_connection_string=$(pulumi stack output -s $stack_name --show-secrets applicationInsightsConnectionString)

echo "ServiceBusConnectionString=$service_bus_connection_string"
echo "ApplicationInsightsConnectionString=$application_insights_connection_string"
