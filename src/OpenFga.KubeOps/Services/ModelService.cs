using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.KubeOps.Services.Resolvers;
using OpenFga.Sdk.Client.Model;
using System.Security.Cryptography;
using System.Text;

namespace OpenFga.KubeOps.Services;

public class ModelService(OpenFgaClientFactory openFgaClientFactory, IKubernetesClient kubernetesClient, AuthorizationStoreResolver authorizationStoreResolver, ILogger<ModelService> logger)
{
    public async Task<AuthorizationModelId> UpdateAuthorizationModelAsync(V1AuthorizationModel model, CancellationToken cancellationToken = default)
    {
        var configRef = model.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.Create(configRef.Name, cancellationToken);

        var storeRef = model.Spec.StoreRef;
        var storeId = await authorizationStoreResolver.ResolveAsync(storeRef.Name, cancellationToken);

        var modelJsonContent = model.Spec.ModelJson;
        var modelJsonHash = ComputeHash(modelJsonContent);

        if (model.Status.ObservedModelHash == modelJsonHash)
        {
            var existingModelId = model.Status.ModelId;
            if (!string.IsNullOrWhiteSpace(existingModelId))
            {
                return model.Status.ModelId;
            }
        }

        logger.LogInformation("Updating authorization model for store {StoreId} with hash {ModelHash}.", storeId, modelJsonHash);

        var clientWriteModelRequest = ClientWriteAuthorizationModelRequest.FromJson(modelJsonContent);
        var clientWriteModelResponse = await openFgaClient.WriteAuthorizationModel(clientWriteModelRequest, cancellationToken: cancellationToken);

        var modelId = clientWriteModelResponse.AuthorizationModelId;

        logger.LogInformation("Updated authorization model for store {StoreId} with new model ID {ModelId}.", storeId, modelId);

        model.Status.ModelId = modelId;
        model.Status.ObservedModelHash = modelJsonHash;

        await kubernetesClient.UpdateStatusAsync(model, cancellationToken);

        return model.Status.ModelId;
    }

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
