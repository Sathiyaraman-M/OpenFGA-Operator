FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /operator

COPY ./ ./
RUN dotnet publish src/OpenFga.KubeOps/ -c Release /p:AssemblyName=operator -o out

# Download the OpenFGA CLI
FROM alpine:latest AS fga-cli

RUN apk add --no-cache curl tar

ARG OPENFGA_CLI_VERSION=0.7.15
ARG TARGETARCH

RUN curl -fsSL \
    "https://github.com/openfga/cli/releases/download/v${OPENFGA_CLI_VERSION}/fga_${OPENFGA_CLI_VERSION}_linux_${TARGETARCH}.tar.gz" \
    | tar -xz -C /usr/local/bin

# The runner for the application
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled-extra AS final

WORKDIR /operator
COPY --from=build /operator/out/ ./
COPY --from=fga-cli /usr/local/bin/fga /usr/local/bin/fga

USER $APP_UID

ENTRYPOINT [ "dotnet", "operator.dll" ]
