using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Abstractions;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.KubeOps.Services.Resolvers;
using System.Security.Cryptography;
using System.Text;

namespace OpenFga.KubeOps.Services;

public class ModelService(OpenFgaService openFgaService, IModelTransformer modelTransformer, AuthorizationStoreResolver authorizationStoreResolver, ILogger<ModelService> logger)
{
    public async Task<UpdateAuthorizationModelResult> UpdateAuthorizationModelAsync(V1FgaAuthorizationModel model, CancellationToken cancellationToken = default)
    {
        var modelDslContent = model.Spec.ModelDsl;
        var modelJsonContent = await modelTransformer.TransformDslToJsonAsync(modelDslContent);
        var modelJsonHash = ComputeHash(modelJsonContent);

        if (model.Status.ObservedModelHash == modelJsonHash)
        {
            var existingModelId = model.Status.ModelId;
            if (!string.IsNullOrWhiteSpace(existingModelId))
            {
                return new UpdateAuthorizationModelResult(model.Status.ModelId, modelJsonHash, model.Status.StoreId);
            }
        }

        var storeRef = model.Spec.StoreRef;
        var storeManifest = await authorizationStoreResolver.ResolveManifestAsync(storeRef.Name, cancellationToken);
        var configRef = storeManifest.Spec.ConnectionConfigRef;

        logger.LogInformation("Updating authorization model for store {StoreName} with hash {ModelHash}.", storeRef.Name, modelJsonHash);

        var modelId = await openFgaService.UpdateAuthorizationModelAsync(modelJsonContent, storeRef.Name, configRef, cancellationToken);

        logger.LogInformation("Updated authorization model for store {StoreName} with new model ID {ModelId}.", storeRef.Name, modelId);

        return new UpdateAuthorizationModelResult(modelId, modelJsonHash, storeManifest.Status.StoreId);
    }

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
