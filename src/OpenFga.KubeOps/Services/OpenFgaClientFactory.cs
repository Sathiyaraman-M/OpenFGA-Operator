using OpenFga.KubeOps.Services.Resolvers;
using OpenFga.Sdk.Client;

namespace OpenFga.KubeOps.Services;

public class OpenFgaClientFactory(ConnectionConfigResolver connectionConfigResolver, AuthorizationStoreResolver authorizationStoreResolver)
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

    public async Task<OpenFgaClient> CreateAsync(string connectionConfigName, string storeRefName, CancellationToken cancellationToken = default)
    {
        var config = await connectionConfigResolver.ResolveAsync(connectionConfigName, cancellationToken);
        var storeId = await authorizationStoreResolver.ResolveAsync(storeRefName, cancellationToken);
        var clientConfiguration = new ClientConfiguration
        {
            ApiUrl = config.ApiUrl,
            StoreId = storeId
        };
        return new OpenFgaClient(clientConfiguration);
    }
}
