using k8s.Models;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.Sdk.Client.Model;

namespace OpenFga.KubeOps.Services;

public class StoreService(OpenFgaClientFactory openFgaClientFactory)
{
    public async Task<StoreId> EnsureStoreExistsAsync(V1AuthorizationStore store, CancellationToken cancellationToken = default)
    {
        var configRef = store.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.Create(configRef.Name, cancellationToken);

        var existingStoreId = store.Status.StoreId;
        if (!string.IsNullOrWhiteSpace(existingStoreId))
        {
            return existingStoreId;
        }

        var storeName = store.Name();

        var listStoresRequest = new ClientListStoresRequest() { Name = storeName };
        var listStoresResponse = await openFgaClient.ListStores(listStoresRequest, cancellationToken: cancellationToken);

        var targetStore = listStoresResponse.Stores.FirstOrDefault(x => x.Name == storeName);

        if (targetStore == null)
        {
            var createStoreRequest = new ClientCreateStoreRequest() { Name = storeName };
            var createStoreResponse = await openFgaClient.CreateStore(createStoreRequest, cancellationToken: cancellationToken);

            return createStoreResponse.Id;
        }

        return targetStore.Id;
    }
}
