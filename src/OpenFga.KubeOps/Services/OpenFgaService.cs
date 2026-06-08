using OpenFga.KubeOps.Models;
using OpenFga.KubeOps.Services.Resolvers;
using OpenFga.Sdk.Client.Model;
using OpenFga.Sdk.Exceptions;

namespace OpenFga.KubeOps.Services;

public class OpenFgaService(OpenFgaClientFactory openFgaClientFactory, AuthorizationStoreResolver authorizationStoreResolver)
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

            var targetStore = listStoresResponse.Stores.FirstOrDefault(x => x.Name == storeName);
            return targetStore != null ? (StoreId)targetStore.Id : null;
        }
        catch (ApiException e)
        {
            throw new StoreQueryFailedException(storeName, e);
        }
    }

    public async Task<AuthorizationModelId> UpdateAuthorizationModelAsync(string modelJson, string storeName, string connectionConfigName, CancellationToken cancellationToken)
    {
        try
        {
            using var openFgaClient = await openFgaClientFactory.CreateAsync(connectionConfigName, cancellationToken);
            var storeId = await authorizationStoreResolver.ResolveAsync(storeName, cancellationToken);

            var clientWriteModelRequest = ClientWriteAuthorizationModelRequest.FromJson(modelJson);
            var clientWriteOptions = new ClientWriteOptions { StoreId = storeId };
            var clientWriteModelResponse = await openFgaClient.WriteAuthorizationModel(clientWriteModelRequest, clientWriteOptions, cancellationToken);

            return clientWriteModelResponse.AuthorizationModelId;
        }
        catch (ApiException e)
        {
            throw new AuthorizationModelUpdateFailedException(storeName, e);
        }
    }
}
