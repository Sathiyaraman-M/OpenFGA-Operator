using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.KubeOps.Services.Resolvers;
using OpenFga.Sdk.Client.Model;

namespace OpenFga.KubeOps.Services;

public class ModelService(OpenFgaClientFactory openFgaClientFactory, AuthorizationStoreResolver authorizationStoreResolver)
{
    public async Task<UpdateAuthorizationModelResult> UpdateAuthorizationModelAsync(V1AuthorizationModel model)
    {
        var configRef = model.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.Create(configRef.Name);

        var storeRef = model.Spec.StoreRef;
        var storeId = await authorizationStoreResolver.ResolveAsync(storeRef.Name);

        var modelJsonContent = model.Spec.ModelJson;

        var clientWriteModelRequest = ClientWriteAuthorizationModelRequest.FromJson(modelJsonContent);
        var clientWriteModelResponse = await openFgaClient.WriteAuthorizationModel(clientWriteModelRequest);

        return new UpdateAuthorizationModelResult(storeId, clientWriteModelResponse.AuthorizationModelId);
    }
}