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
[EntityRbac(typeof(V1FgaAuthorizationModel), Verbs = RbacVerb.All)]
public sealed class ModelController(ModelService modelService, IKubernetesClient kubernetesClient, ILogger<ModelController> logger) : IEntityController<V1FgaAuthorizationModel>
{
    public async Task<ReconciliationResult<V1FgaAuthorizationModel>> ReconcileAsync(V1FgaAuthorizationModel entity, CancellationToken cancellationToken)
    {
        try
        {
            var result = await modelService.UpdateAuthorizationModelAsync(entity, cancellationToken);

            entity.Status.ModelId = result.ModelId;
            entity.Status.ObservedModelHash = result.ModelJsonHash;

            entity.Status.Conditions = [
                V1Condition.New(
                    type: "ConnectionConfigReady",
                    status: "True",
                    reason: "ConnectionConfigFound",
                    message: $"Connection config with name {entity.Spec.ConnectionConfigRef.Name} is found and accessible."
                ),
                V1Condition.New(
                    type: "StoreReady",
                    status: "True",
                    reason: "StoreFound",
                    message: $"Store with name {entity.Spec.StoreRef.Name} is found and accessible."
                ),
                V1Condition.New(
                    type: "ModelReady",
                    status: "True",
                    reason: "ModelUpdateSuccessful",
                    message: $"Authorization model was successfully updated with model ID {result.ModelId}."
                )
            ];
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);
        }
        catch (ConnectionConfigNotFoundException e)
        {
            logger.LogError(e, "Connection config with name {} is not found for OpenFGA Authorization Model {}.", entity.Spec.ConnectionConfigRef.Name, entity.Name());

            entity.Status.Conditions = [
                V1Condition.New(
                    type: "ConnectionConfigReady",
                    status: "False",
                    reason: "ConnectionConfigNotFound",
                    message: e.Message
                )
            ];
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);

            return ReconciliationResult<V1FgaAuthorizationModel>.Failure(entity, e.Message, e);
        }
        catch (AuthorizationStoreNotFoundException e)
        {
            logger.LogError(e, "Store with name {} is not found for OpenFGA Authorization Model {}.", entity.Spec.StoreRef.Name, entity.Name());

            entity.Status.Conditions = [
                V1Condition.New(
                    type: "ConnectionConfigReady",
                    status: "True",
                    reason: "ConnectionConfigFound",
                    message: $"Connection config with name {entity.Spec.ConnectionConfigRef.Name} is found and accessible."
                ),
                V1Condition.New(
                    type: "StoreReady",
                    status: "False",
                    reason: "StoreNotFound",
                    message: e.Message
                )
            ];
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);

            return ReconciliationResult<V1FgaAuthorizationModel>.Failure(entity, e.Message, e);
        }
        catch (AuthorizationModelUpdateFailedException e)
        {
            logger.LogError(e, "Failed to update OpenFGA Authorization Model {} for store {}.", entity.Name(), entity.Spec.StoreRef.Name);

            entity.Status.Conditions = [
                V1Condition.New(
                    type: "ConnectionConfigReady",
                    status: "True",
                    reason: "ConnectionConfigFound",
                    message: $"Connection config with name {entity.Spec.ConnectionConfigRef.Name} is found and accessible."
                ),
                V1Condition.New(
                    type: "StoreReady",
                    status: "False",
                    reason: "StoreNotFound",
                    message: e.Message
                ),
                V1Condition.New(
                    type: "ModelReady",
                    status: "False",
                    reason: "ModelUpdateFailed",
                    message: e.Message
                )
            ];
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);

            return ReconciliationResult<V1FgaAuthorizationModel>.Failure(entity, e.Message, e);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Something went wrong while updating OpenFGA Authorization Model {}", entity.Name());

            entity.Status.Conditions = [
                V1Condition.New(
                    type: "ModelReady",
                    status: "False",
                    reason: "UnexpectedError",
                    message: e.Message
                )
            ];
            await kubernetesClient.UpdateStatusAsync(entity, cancellationToken);

            return ReconciliationResult<V1FgaAuthorizationModel>.Failure(entity, e.Message, e);
        }

        return ReconciliationResult<V1FgaAuthorizationModel>.Success(entity);
    }

    public async Task<ReconciliationResult<V1FgaAuthorizationModel>> DeletedAsync(V1FgaAuthorizationModel entity, CancellationToken cancellationToken)
    {
        logger.LogWarning("The cluster object for OpenFGA Model {} with ID {} is removed. The actual OpenFGA Authorization Models are immutable so they can't be removed.", entity.Name(), entity.Status.ModelId);
        return ReconciliationResult<V1FgaAuthorizationModel>.Success(entity);
    }
}
