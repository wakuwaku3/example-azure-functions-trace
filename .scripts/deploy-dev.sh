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

set -eu

# エラーハンドリング
trap 'echo "Error occurred at line $LINENO. Exiting."; exit 1;' ERR


pulumi up -s $stack_name -y
resource_group_name=$(pulumi stack output -s $stack_name resourceGroupName)
function_app_name=$(pulumi stack output -s $stack_name functionAppName)

echo "ResourceGroupName=$resource_group_name"
echo "FunctionAppName=$function_app_name"

cd $root_path/server/Function
func azure functionapp publish $function_app_name
