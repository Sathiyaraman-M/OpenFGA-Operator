using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.KubeOps.Services.Resolvers;
using OpenFga.Sdk.Client.Model;
using System.Security.Cryptography;
using System.Text;

namespace OpenFga.KubeOps.Services;

public class ModelService(OpenFgaClientFactory openFgaClientFactory, AuthorizationStoreResolver authorizationStoreResolver, ILogger<ModelService> logger)
{
    public async Task<UpdateAuthorizationModelResult> UpdateAuthorizationModelAsync(V1AuthorizationModel model, CancellationToken cancellationToken = default)
    {
        var storeRef = model.Spec.StoreRef;
        var storeId = await authorizationStoreResolver.ResolveAsync(storeRef.Name, cancellationToken);

        var configRef = model.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.CreateAsync(configRef.Name, storeId, cancellationToken);

        var modelJsonContent = model.Spec.ModelJson;
        var modelJsonHash = ComputeHash(modelJsonContent);

        if (model.Status.ObservedModelHash == modelJsonHash)
        {
            var existingModelId = model.Status.ModelId;
            if (!string.IsNullOrWhiteSpace(existingModelId))
            {
                return new UpdateAuthorizationModelResult(model.Status.ModelId, modelJsonHash);
            }
        }

        logger.LogInformation("Updating authorization model for store {StoreId} with hash {ModelHash}.", storeId, modelJsonHash);

        var clientWriteModelRequest = ClientWriteAuthorizationModelRequest.FromJson(modelJsonContent);
        var clientWriteModelResponse = await openFgaClient.WriteAuthorizationModel(clientWriteModelRequest, cancellationToken: cancellationToken);

        var modelId = clientWriteModelResponse.AuthorizationModelId;

        logger.LogInformation("Updated authorization model for store {StoreId} with new model ID {ModelId}.", storeId, modelId);

        return new UpdateAuthorizationModelResult(modelId, modelJsonHash);
    }

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
