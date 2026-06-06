using KubeOps.KubernetesClient;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;

namespace OpenFga.KubeOps.Services.Resolvers;

public class ConnectionConfigResolver(IKubernetesClient client)
{
    public async Task<ConnectionConfig> ResolveAsync(string configName)
    {
        var config = await client.GetAsync<V1ConnectionConfig>(configName)
            ?? throw new ConnectionConfigNotFoundException(configName);

        return new ConnectionConfig
        {
            ApiUrl = config.Spec.ApiUrl
        };
    }
}
