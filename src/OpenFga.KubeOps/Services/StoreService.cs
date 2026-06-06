using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.Sdk.Client.Model;

namespace OpenFga.KubeOps.Services;

public class StoreService(OpenFgaClientFactory openFgaClientFactory, IKubernetesClient kubernetesClient, ILogger<StoreService> logger)
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
        if (targetStore != null)
        {
            logger.LogInformation("Store with name {StoreName} already exists in OpenFGA with ID {StoreId}.", storeName, targetStore.Id);
            store.Status.StoreId = targetStore.Id;
        }
        else
        {
            logger.LogInformation("Store with name {StoreName} not found in OpenFGA. Creating new store.", storeName);
            var createStoreRequest = new ClientCreateStoreRequest() { Name = storeName };
            var createStoreResponse = await openFgaClient.CreateStore(createStoreRequest, cancellationToken: cancellationToken);

            logger.LogInformation("Created new store with name {StoreName} in OpenFGA with ID {StoreId}.", storeName, createStoreResponse.Id);
            store.Status.StoreId = createStoreResponse.Id;
        }

        await kubernetesClient.UpdateStatusAsync(store, cancellationToken);

        return store.Status.StoreId;
    }
}
