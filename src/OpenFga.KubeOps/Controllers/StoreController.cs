using k8s.Models;
using KubeOps.Abstractions.Rbac;
using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Controller;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Extensions;
using OpenFga.KubeOps.Services;

namespace OpenFga.KubeOps.Controllers;

[EntityRbac(typeof(V1FgaConnectionConfig), Verbs = RbacVerb.Get | RbacVerb.List | RbacVerb.Watch)]
[EntityRbac(typeof(V1FgaStore), Verbs = RbacVerb.All)]
public sealed class StoreController(StoreService storeService, IKubernetesClient kubernetesClient, ILogger<StoreController> logger) : IEntityController<V1FgaStore>
{
    public async Task<ReconciliationResult<V1FgaStore>> ReconcileAsync(V1FgaStore entity, CancellationToken cancellationToken)
    {
        try
        {
            var storeId = await storeService.EnsureStoreExistsAsync(entity, cancellationToken);
            entity.Status.StoreId = storeId;

            entity.Status.Conditions = [
                V1Condition.New(
                    type: "ConnectionConfigReady",
                    status: "True",
                    reason: "ConnectionConfigFound",
                    message: $"Connection config with name {entity.Spec.ConnectionConfigRef} is found and accessible."
                ),
                V1Condition.New(
                    type: "StoreReady",
                    status: "True",
                    reason: "StoreReconciliationSuccessful",
                    message: $"Store with name {entity.Name()} is reconciled successfully with store ID {storeId}."
                )
            ];
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);
        }
        catch (ConnectionConfigNotFoundException e)
        {
            logger.LogError(e, "Error while reconciling OpenFGA Store {}", entity.Name());

            entity.Status.Conditions = [
                V1Condition.New(
                    type: "ConnectionConfigReady",
                    status: "False",
                    reason: "ConnectionConfigNotFound",
                    message: e.Message
                )
            ];
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);

            return ReconciliationResult<V1FgaStore>.Failure(entity, e.Message, e);
        }
        catch (StoreCreationFailedException e)
        {
            logger.LogError(e, "Error while reconciling OpenFGA Store {}", entity.Name());

            entity.Status.Conditions = [
                V1Condition.New(
                    type: "ConnectionConfigReady",
                    status: "True",
                    reason: "ConnectionConfigFound",
                    message: $"Connection config with name {entity.Spec.ConnectionConfigRef} is found and accessible."
                ),
                V1Condition.New(
                    type: "StoreReady",
                    status: "False",
                    reason: "StoreCreationFailed",
                    message: e.Message
                )
            ];
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);

            return ReconciliationResult<V1FgaStore>.Failure(entity, e.Message, e);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went wrong while reconciling OpenFGA Store {}", entity.Name());

            entity.Status.Conditions = [
                V1Condition.New(
                    type: "StoreReady",
                    status: "False",
                    reason: "UnexpectedError",
                    message: e.Message
                )
            ];
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);

            return ReconciliationResult<V1FgaStore>.Failure(entity, e.Message, e);
        }

        return ReconciliationResult<V1FgaStore>.Success(entity);
    }

    public async Task<ReconciliationResult<V1FgaStore>> DeletedAsync(V1FgaStore entity, CancellationToken cancellationToken)
    {
        logger.LogWarning("The cluster object for OpenFGA Store {} with ID {} is removed. The actual store will not be removed automatically. Please manually remove the store from OpenFGA.", entity.Name(), entity.Status.StoreId);
        return ReconciliationResult<V1FgaStore>.Success(entity);
    }
}
