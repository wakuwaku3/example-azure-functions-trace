FROM mcr.microsoft.com/devcontainers/dotnet:8.0-bookworm

# install azure cli
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# install azure functions core tools
RUN export DEBIAN_VERSION=12 \
    && apt-get update && apt-get install gpg wget -y \
    && wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor | tee /usr/share/keyrings/microsoft-prod.gpg \
    && wget -q https://packages.microsoft.com/config/debian/$DEBIAN_VERSION/prod.list \
    && mv prod.list /etc/apt/sources.list.d/microsoft-prod.list \
    && chown root:root /usr/share/keyrings/microsoft-prod.gpg \
    && chown root:root /etc/apt/sources.list.d/microsoft-prod.list \
    && apt-get update && apt-get install azure-functions-core-tools-4 -y \
    && apt-get update && apt-get install libicu-dev -y

WORKDIR /home/vscode
USER vscode

# pulumi
RUN curl -fsSL https://get.pulumi.com | sh
ENV PATH="/home/vscode/.pulumi/bin:$PATH"
