using OpenFga.KubeOps.Services.Resolvers;
using OpenFga.Sdk.Client;

namespace OpenFga.KubeOps.Services;

public class OpenFgaClientFactory(ConnectionConfigResolver connectionConfigResolver)
{
    public async Task<OpenFgaClient> Create(string connectionConfigName)
    {
        var config = await connectionConfigResolver.ResolveAsync(connectionConfigName);
        var clientConfiguration = new ClientConfiguration
        {
            ApiUrl = config.ApiUrl
        };
        return new OpenFgaClient(clientConfiguration);
    }
}
