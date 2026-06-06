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

            var storeExistsCondition = new V1Condition()
            {
                Type = "StoreExists",
                Status = "True",
                Reason = "StoreAlreadyExists",
                Message = $"Store with name {storeName} already exists in OpenFGA with ID {targetStore.Id}."
            };

            store.Status.Conditions.Add(storeExistsCondition);

            store.Status.StoreId = targetStore.Id;
        }
        else
        {
            logger.LogInformation("Store with name {StoreName} not found in OpenFGA. Creating new store.", storeName);

            var storeNotFoundCondition = new V1Condition()
            {
                Type = "StoreNotFound",
                Status = "True",
                Reason = "StoreMissing",
                Message = $"Store with name {storeName} was not found in OpenFGA. A new store will be created."
            };

            store.Status.Conditions.Add(storeNotFoundCondition);

            var createStoreRequest = new ClientCreateStoreRequest() { Name = storeName };
            var createStoreResponse = await openFgaClient.CreateStore(createStoreRequest, cancellationToken: cancellationToken);

            logger.LogInformation("Created new store with name {StoreName} in OpenFGA with ID {StoreId}.", storeName, createStoreResponse.Id);
            store.Status.StoreId = createStoreResponse.Id;

            var storeCreatedCondition = new V1Condition()
            {
                Type = "StoreCreated",
                Status = "True",
                Reason = "StoreCreationSuccessful",
                Message = $"Store with name {storeName} has been created in OpenFGA with ID {createStoreResponse.Id}."
            };

            store.Status.Conditions.Add(storeCreatedCondition);
        }

        await kubernetesClient.UpdateStatusAsync(store, cancellationToken);

        return store.Status.StoreId;
    }
}
