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

pulumi destroy -s -y $stack_name
