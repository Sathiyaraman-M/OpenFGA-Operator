using KubeOps.KubernetesClient;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;

namespace OpenFga.KubeOps.Services.Resolvers;

public class AuthorizationStoreResolver(IKubernetesClient client)
{
    public async Task<StoreId> ResolveAsync(string storeName)
    {
        var config = await client.GetAsync<V1AuthorizationStore>(storeName)
            ?? throw new AuthorizationStoreNotFoundException(storeName);

        return config.Status.StoreId;
    }
}
