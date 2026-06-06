using k8s.Models;
using KubeOps.Abstractions.Rbac;
using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Controller;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Services;

namespace OpenFga.KubeOps.Controllers;

[EntityRbac(typeof(V1AuthorizationStore), Verbs = RbacVerb.All)]
public sealed class StoreController(StoreService storeService, ILogger<StoreController> logger) : IEntityController<V1AuthorizationStore>
{
    public async Task<ReconciliationResult<V1AuthorizationStore>> ReconcileAsync(V1AuthorizationStore entity, CancellationToken cancellationToken)
    {
        string? storeId;
        try
        {
            storeId = await storeService.EnsureStoreExistsAsync(entity);
        }
        catch (ConnectionConfigNotFoundException e)
        {
            logger.LogError(e, "Error while reconciling OpenFGA Store {}", entity.Name());
            return ReconciliationResult<V1AuthorizationStore>.Failure(entity, e.Message, e);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went wrong while reconciling OpenFGA Store {}", entity.Name());
            return ReconciliationResult<V1AuthorizationStore>.Failure(entity, e.Message, e);
        }

        entity.Status.StoreId = storeId;

        return ReconciliationResult<V1AuthorizationStore>.Success(entity);
    }

    public async Task<ReconciliationResult<V1AuthorizationStore>> DeletedAsync(V1AuthorizationStore entity, CancellationToken cancellationToken)
    {
        logger.LogWarning("The cluster object for OpenFGA Store {} with ID {} is removed. The actual store will not be removed automatically. Please manually remove the store from OpenFGA.", entity.Name(), entity.Status.StoreId);
        return ReconciliationResult<V1AuthorizationStore>.Success(entity);
    }
}
