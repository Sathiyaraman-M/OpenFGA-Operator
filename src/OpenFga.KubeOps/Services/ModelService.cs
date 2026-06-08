using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.KubeOps.Services.Resolvers;
using System.Security.Cryptography;
using System.Text;

namespace OpenFga.KubeOps.Services;

public class ModelService(OpenFgaService openFgaService, AuthorizationStoreResolver authorizationStoreResolver, ILogger<ModelService> logger)
{
    public async Task<UpdateAuthorizationModelResult> UpdateAuthorizationModelAsync(V1AuthorizationModel model, CancellationToken cancellationToken = default)
    {
        var storeRef = model.Spec.StoreRef;
        var storeId = await authorizationStoreResolver.ResolveAsync(storeRef.Name, cancellationToken);

        var configRef = model.Spec.ConnectionConfigRef;

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

        var modelId = await openFgaService.UpdateAuthorizationModelAsync(storeId, modelJsonContent, configRef.Name, cancellationToken);

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
