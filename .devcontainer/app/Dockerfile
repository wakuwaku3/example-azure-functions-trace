FROM mcr.microsoft.com/devcontainers/dotnet

# install azure cli
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

WORKDIR /home/vscode
USER vscode

# pulumi
RUN curl -fsSL https://get.pulumi.com | sh
ENV PATH="/home/vscode/.pulumi/bin:$PATH"
