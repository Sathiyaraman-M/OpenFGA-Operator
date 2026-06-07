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

[EntityRbac(typeof(V1ConnectionConfig), Verbs = RbacVerb.Get | RbacVerb.List | RbacVerb.Watch)]
[EntityRbac(typeof(V1AuthorizationModel), Verbs = RbacVerb.All)]
public sealed class ModelController(ModelService modelService, IKubernetesClient kubernetesClient, ILogger<ModelController> logger) : IEntityController<V1AuthorizationModel>
{
    public async Task<ReconciliationResult<V1AuthorizationModel>> ReconcileAsync(V1AuthorizationModel entity, CancellationToken cancellationToken)
    {
        try
        {
            var result = await modelService.UpdateAuthorizationModelAsync(entity, cancellationToken);

            entity.Status.ModelId = result.ModelId;
            entity.Status.ObservedModelHash = result.ModelJsonHash;

            entity.Status.Conditions.SetCondition(
                type: "Ready",
                status: "True",
                reason: "ModelUpdateSuccessful",
                message: $"Authorization model was successfully updated with model ID {result.ModelId}."
            );
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);
        }
        catch (ConnectionConfigNotFoundException e)
        {
            logger.LogError(e, "Error while updating OpenFGA Authorization Model {}", entity.Name());

            entity.Status.Conditions.SetCondition(
                type: "Ready",
                status: "False",
                reason: "ConnectionConfigNotFound",
                message: e.Message
            );
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);

            return ReconciliationResult<V1AuthorizationModel>.Failure(entity, e.Message, e);
        }
        catch (AuthorizationStoreNotFoundException e)
        {
            logger.LogError(e, "Error while updating OpenFGA Authorization Model {}", entity.Name());

            entity.Status.Conditions.SetCondition(
                type: "Ready",
                status: "False",
                reason: "AuthorizationStoreNotFound",
                message: e.Message
            );
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);

            return ReconciliationResult<V1AuthorizationModel>.Failure(entity, e.Message, e);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went wrong while updating OpenFGA Authorization Model {}", entity.Name());

            entity.Status.Conditions.SetCondition(
                type: "Ready",
                status: "False",
                reason: "ModelUpdateError",
                message: e.Message
            );
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);

            return ReconciliationResult<V1AuthorizationModel>.Failure(entity, e.Message, e);
        }

        return ReconciliationResult<V1AuthorizationModel>.Success(entity);
    }

    public async Task<ReconciliationResult<V1AuthorizationModel>> DeletedAsync(V1AuthorizationModel entity, CancellationToken cancellationToken)
    {
        logger.LogWarning("The cluster object for OpenFGA Model {} with ID {} is removed. The actual OpenFGA Authorization Models are immutable so they can't be removed.", entity.Name(), entity.Status.ModelId);
        return ReconciliationResult<V1AuthorizationModel>.Success(entity);
    }
}
