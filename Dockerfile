FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /operator

COPY ./ ./
RUN dotnet publish src/OpenFga.KubeOps/ -c Release /p:AssemblyName=operator -o out

# Build the OpenFGA CLI
FROM golang:1.25 AS fga-cli

RUN go install github.com/openfga/cli/cmd/fga@latest

# The runner for the application
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled-extra AS final

WORKDIR /operator
COPY --from=build /operator/out/ ./
COPY --from=fga-cli /go/bin/fga /usr/local/bin/fga

USER $APP_UID

ENTRYPOINT [ "dotnet", "operator.dll" ]
