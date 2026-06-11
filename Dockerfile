FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /operator

COPY ./ ./
RUN dotnet publish src/OpenFga.KubeOps/ -c Release /p:AssemblyName=operator -o out

# The runner for the application
FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled-extra AS final

WORKDIR /operator
COPY --from=build /operator/out/ ./

USER $APP_UID

ENTRYPOINT [ "dotnet", "operator.dll" ]