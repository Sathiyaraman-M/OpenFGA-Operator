using System.Security.Cryptography;
using System.Text;
using KubeOps.KubernetesClient;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.KubeOps.Services.Resolvers;
using OpenFga.Sdk.Client.Model;

namespace OpenFga.KubeOps.Services;

public class ModelService(OpenFgaClientFactory openFgaClientFactory, IKubernetesClient kubernetesClient, AuthorizationStoreResolver authorizationStoreResolver)
{
    public async Task<AuthorizationModelId> UpdateAuthorizationModelAsync(V1AuthorizationModel model, CancellationToken cancellationToken = default)
    {
        var configRef = model.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.Create(configRef.Name, cancellationToken);

        var storeRef = model.Spec.StoreRef;
        var storeId = await authorizationStoreResolver.ResolveAsync(storeRef.Name, cancellationToken);

        var modelJsonContent = model.Spec.ModelJson;

        var clientWriteModelRequest = ClientWriteAuthorizationModelRequest.FromJson(modelJsonContent);
        var clientWriteModelResponse = await openFgaClient.WriteAuthorizationModel(clientWriteModelRequest, cancellationToken: cancellationToken);

        var modelId = clientWriteModelResponse.AuthorizationModelId;

        model.Status.ModelId = modelId;
        var modelHashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(modelJsonContent));
        model.Status.ObservedModelHash = Encoding.UTF8.GetString(modelHashBytes);

        await kubernetesClient.UpdateStatusAsync(model, cancellationToken);

        return model.Status.ModelId;
    }
}
