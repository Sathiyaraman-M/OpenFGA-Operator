using k8s.Models;
using KubeOps.Abstractions.Events;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.Sdk.Client.Model;

namespace OpenFga.KubeOps.Services;

public class StoreService(OpenFgaClientFactory openFgaClientFactory, EventPublisher eventPublisher, ILogger<StoreService> logger)
{
    public async Task<StoreId> EnsureStoreExistsAsync(V1AuthorizationStore store, CancellationToken cancellationToken = default)
    {
        var configRef = store.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.CreateAsync(configRef.Name, cancellationToken);

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

            await eventPublisher(
                entity: store,
                reason: "StoreAlreadyExists",
                message: $"Store with name {storeName} already exists in OpenFGA with ID {targetStore.Id}. No action is needed.",
                type: EventType.Normal,
                cancellationToken: cancellationToken
            );

            return targetStore.Id;
        }

        logger.LogInformation("Store with name {StoreName} not found in OpenFGA. Creating new store.", storeName);

        await eventPublisher(
            entity: store,
            reason: "StoreCreationStarted",
            message: $"Store with name {storeName} not found in OpenFGA. Starting creation of new store.",
            type: EventType.Normal,
            cancellationToken: cancellationToken
        );

        var createStoreRequest = new ClientCreateStoreRequest() { Name = storeName };
        var createStoreResponse = await openFgaClient.CreateStore(createStoreRequest, cancellationToken: cancellationToken);

        logger.LogInformation("Created new store with name {StoreName} in OpenFGA with ID {StoreId}.", storeName, createStoreResponse.Id);

        await eventPublisher(
            entity: store,
            reason: "StoreCreationSuccessful",
            message: $"Store with name {storeName} has been created in OpenFGA with ID {createStoreResponse.Id}. The store is now ready to be used.",
            type: EventType.Normal,
            cancellationToken: cancellationToken
        );

        return createStoreResponse.Id;
    }
}
