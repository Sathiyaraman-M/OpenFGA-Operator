using OpenFga.KubeOps.Models;
using OpenFga.KubeOps.Services.Resolvers;
using OpenFga.Sdk.Client;

namespace OpenFga.KubeOps.Services;

public class OpenFgaClientFactory(ConnectionConfigResolver connectionConfigResolver)
{
    public async Task<OpenFgaClient> CreateAsync(string connectionConfigName, CancellationToken cancellationToken = default)
    {
        var config = await connectionConfigResolver.ResolveAsync(connectionConfigName, cancellationToken);
        var clientConfiguration = new ClientConfiguration
        {
            ApiUrl = config.ApiUrl,
            StoreId = null
        };
        return new OpenFgaClient(clientConfiguration);
    }

    public async Task<OpenFgaClient> CreateAsync(string connectionConfigName, StoreId storeId, CancellationToken cancellationToken = default)
    {
        var config = await connectionConfigResolver.ResolveAsync(connectionConfigName, cancellationToken);
        var clientConfiguration = new ClientConfiguration
        {
            ApiUrl = config.ApiUrl,
            StoreId = storeId
        };
        return new OpenFgaClient(clientConfiguration);
    }
}
