using k8s.Models;
using OpenFga.KubeOps.Entities;
using OpenFga.Sdk.Client.Model;

namespace OpenFga.KubeOps.Services;

public class StoreService(OpenFgaClientFactory openFgaClientFactory)
{
    public async Task<string> EnsureStoreExistsAsync(V1AuthorizationStore store)
    {
        var configRef = store.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.Create(configRef.Name);
        var storeName = store.Name();

        var listStoresRequest = new ClientListStoresRequest() { Name = storeName };
        var listStoresResponse = await openFgaClient.ListStores(listStoresRequest);

        var targetStore = listStoresResponse.Stores.FirstOrDefault(x => x.Name == storeName);

        if (targetStore == null)
        {
            var createStoreRequest = new ClientCreateStoreRequest() { Name = storeName };
            var createStoreResponse = await openFgaClient.CreateStore(createStoreRequest);

            return createStoreResponse.Id;
        }

        return targetStore.Id;
    }
}
