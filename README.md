# example-azure-functions-trace

## installation

このリポジトリは vscode の dev container で開発することを前提としています。開発するには以下のツールと環境変数を設定する必要があります。

### tools

- vscode
- docker

### env

- EXAMPLE_AZURE_FUNCTIONS_TRACE_PULUMI_ACCESS_TOKEN  
  pulumi のアクセストークンです。 [pulumi の公式サイト](https://app.pulumi.com/signin)から取得できます。
- EXAMPLE_AZURE_FUNCTIONS_TRACE_AZURE_RESOURCE_PREFIX
  Azure リソースのプレフィックスです。

## commands

### Azure にログインする

pulumi を使って Azure にデプロイしているため、事前に Azure cli を使ってログインする必要があります。

```sh
az login
```

### デプロイする

```sh
./.scripts/deploy-dev.sh
```

### 削除する

```sh
./.scripts/destroy-dev.sh
```
