using KubeOps.KubernetesClient;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;

namespace OpenFga.KubeOps.Services.Resolvers;

public class ConnectionConfigResolver(IKubernetesClient client)
{
    public async Task<ConnectionConfig> ResolveAsync(string configName, CancellationToken cancellationToken = default)
    {
        var config = await client.GetAsync<V1FgaConnectionConfig>(configName, cancellationToken: cancellationToken)
            ?? throw new ConnectionConfigNotFoundException(configName);

        return new ConnectionConfig
        {
            ApiUrl = config.Spec.ApiUrl
        };
    }
}
