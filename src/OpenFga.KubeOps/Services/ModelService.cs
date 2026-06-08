using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using System.Security.Cryptography;
using System.Text;

namespace OpenFga.KubeOps.Services;

public class ModelService(OpenFgaService openFgaService, ILogger<ModelService> logger)
{
    public async Task<UpdateAuthorizationModelResult> UpdateAuthorizationModelAsync(V1AuthorizationModel model, CancellationToken cancellationToken = default)
    {
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

        var configRef = model.Spec.ConnectionConfigRef;
        var storeRef = model.Spec.StoreRef;

        logger.LogInformation("Updating authorization model for store {StoreName} with hash {ModelHash}.", storeRef.Name, modelJsonHash);

        var modelId = await openFgaService.UpdateAuthorizationModelAsync(modelJsonContent, storeRef.Name, configRef.Name, cancellationToken);

        logger.LogInformation("Updated authorization model for store {StoreName} with new model ID {ModelId}.", storeRef.Name, modelId);

        return new UpdateAuthorizationModelResult(modelId, modelJsonHash);
    }

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
