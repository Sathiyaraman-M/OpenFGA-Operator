using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.Sdk.Client.Model;
using OpenFga.Sdk.Exceptions;

namespace OpenFga.KubeOps.Services;

public class OpenFgaService(OpenFgaClientFactory openFgaClientFactory)
{
    public async Task<StoreId> CreateStoreAsync(string storeName, string connectionConfigName, CancellationToken cancellationToken)
    {
        try
        {
            using var openFgaClient = await openFgaClientFactory.CreateAsync(connectionConfigName, cancellationToken);

            var createStoreRequest = new ClientCreateStoreRequest() { Name = storeName };
            var createStoreResponse = await openFgaClient.CreateStore(createStoreRequest, cancellationToken: cancellationToken);

            return createStoreResponse.Id;
        }
        catch (ApiException e)
        {
            throw new StoreCreationFailedException(storeName, e);
        }
    }

    public async Task<StoreId?> GetStoreIdByNameAsync(string storeName, string connectionConfigName, CancellationToken cancellationToken)
    {
        try
        {
            using var openFgaClient = await openFgaClientFactory.CreateAsync(connectionConfigName, cancellationToken);

            var listStoresRequest = new ClientListStoresRequest() { Name = storeName };
            var listStoresResponse = await openFgaClient.ListStores(listStoresRequest, cancellationToken: cancellationToken);

            if (listStoresResponse.Stores.Count == 0)
            {
                return null;
            }

            if (listStoresResponse.Stores.Count > 1)
            {
                throw new MultipleStoresFoundException(storeName);
            }

            var targetStore = listStoresResponse.Stores.First();
            return targetStore.Id;
        }
        catch (ApiException e)
        {
            throw new StoreQueryFailedException(storeName, e);
        }
    }

    public async Task<bool> CheckIfStoreExistsAsync(StoreId storeId, string connectionConfigName, CancellationToken cancellationToken)
    {
        try
        {
            using var openFgaClient = await openFgaClientFactory.CreateAsync(connectionConfigName, cancellationToken);

            var getStoreOptions = new ClientReadOptions() { StoreId = storeId };
            var getStoreResponse = await openFgaClient.GetStore(getStoreOptions, cancellationToken);

            return getStoreResponse.Id == storeId;
        }
        catch (FgaApiNotFoundError)
        {
            throw new AuthorizationStoreNotFoundException(storeId);
        }
    }

    public async Task<AuthorizationModelId> UpdateAuthorizationModelAsync(string modelJson, string storeName, string connectionConfigName, CancellationToken cancellationToken)
    {
        try
        {
            using var openFgaClient = await openFgaClientFactory.CreateAsync(connectionConfigName, storeName, cancellationToken);

            var clientWriteModelRequest = ClientWriteAuthorizationModelRequest.FromJson(modelJson);
            var clientWriteModelResponse = await openFgaClient.WriteAuthorizationModel(clientWriteModelRequest, cancellationToken: cancellationToken);

            return clientWriteModelResponse.AuthorizationModelId;
        }
        catch (ApiException e)
        {
            throw new AuthorizationModelUpdateFailedException(storeName, e);
        }
    }

    public async Task<TuplesWriteResponse> WriteTuplesAsync(TuplesReconcilationPlan reconcilationPlan, IReadOnlyList<V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState> existingStates,
        string storeName, string connectionConfigName, CancellationToken cancellationToken)
    {
        try
        {
            using var openFgaClient = await openFgaClientFactory.CreateAsync(connectionConfigName, storeName, cancellationToken);

            var clientWriteRequest = new ClientWriteRequest([.. reconcilationPlan.TuplesToAdd], [.. reconcilationPlan.TuplesToRemove]);
            var clientWriteResponse = await openFgaClient.Write(clientWriteRequest, cancellationToken: cancellationToken);

            return TuplesWriteResponse.Create(clientWriteResponse, reconcilationPlan, existingStates);
        }
        catch (ApiException e)
        {
            throw new TuplesWriteFailedException(storeName, e);
        }
    }
}
