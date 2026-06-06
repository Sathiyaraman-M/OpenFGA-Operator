using k8s.Models;
using KubeOps.Abstractions.Rbac;
using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Controller;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Services;

namespace OpenFga.KubeOps.Controllers;

[EntityRbac(typeof(V1AuthorizationModel), Verbs = RbacVerb.All)]
public sealed class ModelController(ModelService modelService, ILogger<ModelController> logger) : IEntityController<V1AuthorizationModel>
{
    public async Task<ReconciliationResult<V1AuthorizationModel>> ReconcileAsync(V1AuthorizationModel entity, CancellationToken cancellationToken)
    {
        string? storeId, modelId;
        try
        {
            var result = await modelService.UpdateAuthorizationModelAsync(entity);
            storeId = result.StoreId;
            modelId = result.ModelId;
        }
        catch (ConnectionConfigNotFoundException e)
        {
            logger.LogError(e, "Error while updating OpenFGA Authorization Model {}", entity.Name());
            return ReconciliationResult<V1AuthorizationModel>.Failure(entity, e.Message, e);
        }
        catch (AuthorizationStoreNotFoundException e)
        {
            logger.LogError(e, "Error while updating OpenFGA Authorization Model {}", entity.Name());
            return ReconciliationResult<V1AuthorizationModel>.Failure(entity, e.Message, e);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went wrong while updating OpenFGA Authorization Model {}", entity.Name());
            return ReconciliationResult<V1AuthorizationModel>.Failure(entity, e.Message, e);
        }

        entity.Status.StoreId = storeId;

        return ReconciliationResult<V1AuthorizationModel>.Success(entity);
    }

    public async Task<ReconciliationResult<V1AuthorizationModel>> DeletedAsync(V1AuthorizationModel entity, CancellationToken cancellationToken)
    {
        logger.LogWarning("The cluster object for OpenFGA Model {} with ID {} is removed. The actual OpenFGA Authorization Models are immutable so they can't be removed.", entity.Name(), entity.Status.ModelId);
        return ReconciliationResult<V1AuthorizationModel>.Success(entity);
    }
}
