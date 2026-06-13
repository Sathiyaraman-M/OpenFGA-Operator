using k8s.Models;
using KubeOps.Abstractions.Events;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;

namespace OpenFga.KubeOps.Services;

public class StoreService(OpenFgaService openFgaService, EventPublisher eventPublisher, ILogger<StoreService> logger)
{
    public async Task<StoreId> EnsureStoreExistsAsync(V1FgaStore store, CancellationToken cancellationToken = default)
    {
        var configRef = store.Spec.ConnectionConfigRef;

        var existingStoreId = store.Status.StoreId;
        if (!string.IsNullOrWhiteSpace(existingStoreId))
        {
            return existingStoreId;
        }

        var storeName = store.Name();

        var storeId = await openFgaService.GetStoreIdByNameAsync(storeName, configRef, cancellationToken);
        if (storeId != null)
        {
            logger.LogInformation("Store with name {StoreName} already exists in OpenFGA with ID {StoreId}.", storeName, storeId);

            await eventPublisher(
                entity: store,
                reason: "StoreAlreadyExists",
                message: $"Store with name {storeName} already exists in OpenFGA with ID {storeId}. No action is needed.",
                type: EventType.Normal,
                cancellationToken: cancellationToken
            );

            return storeId;
        }

        logger.LogInformation("Store with name {StoreName} not found in OpenFGA. Creating new store.", storeName);

        return await CreateStoreAsync(store, storeName, configRef, cancellationToken);
    }

    private async Task<StoreId> CreateStoreAsync(V1FgaStore store, string storeName, string configName, CancellationToken cancellationToken = default)
    {
        await eventPublisher(
            entity: store,
            reason: "StoreCreationStarted",
            message: $"Store with name {storeName} not found in OpenFGA. Starting creation of new store.",
            type: EventType.Normal,
            cancellationToken: cancellationToken
        );

        var storeId = await openFgaService.CreateStoreAsync(storeName, configName, cancellationToken);

        logger.LogInformation("Created new store with name {StoreName} in OpenFGA with ID {StoreId}.", storeName, storeId);

        await eventPublisher(
            entity: store,
            reason: "StoreCreationSuccessful",
            message: $"Store with name {storeName} has been created in OpenFGA with ID {storeId}. The store is now ready to be used.",
            type: EventType.Normal,
            cancellationToken: cancellationToken
        );

        return storeId;
    }
}
