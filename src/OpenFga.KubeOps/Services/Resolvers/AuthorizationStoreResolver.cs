using KubeOps.KubernetesClient;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;

namespace OpenFga.KubeOps.Services.Resolvers;

public class AuthorizationStoreResolver(IKubernetesClient client)
{
    public async Task<StoreId> ResolveAsync(string storeName, CancellationToken cancellationToken = default)
    {
        var config = await client.GetAsync<V1AuthorizationStore>(storeName, cancellationToken: cancellationToken)
            ?? throw new AuthorizationStoreNotFoundException(storeName);

        return config.Status.StoreId;
    }
}
