using k8s.Models;
using KubeOps.Abstractions.Events;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.KubeOps.Services.Resolvers;

namespace OpenFga.KubeOps.Services;

public class TupleSetService(OpenFgaService openFgaService, AuthorizationStoreResolver authorizationStoreResolver, EventPublisher eventPublisher, ILogger<TupleSetService> logger)
{
    public async Task<ReconcileTupleSetResult> ReconcileTupleSetAsync(V1FgaTupleSet tupleSet, CancellationToken cancellationToken = default)
    {
        var storeRef = tupleSet.Spec.StoreRef;
        var storeManifest = await authorizationStoreResolver.ResolveManifestAsync(storeRef.Name, cancellationToken);
        var configRef = storeManifest.Spec.ConnectionConfigRef;

        var reconcilationPlan = TuplesReconcilationPlan.CreateForWrite(tupleSet);
        if (reconcilationPlan.TuplesToAdd.Count == 0 && reconcilationPlan.TuplesToRemove.Count == 0)
        {
            return new ReconcileTupleSetResult(true, tupleSet.Status.StoreId, [.. reconcilationPlan.DesiredStates]);
        }

        logger.LogInformation("Reconciling tuple set {TupleSetName} for store {StoreRefName}. Adding {AddCount} tuples and removing {RemoveCount} tuples.", tupleSet.Name(), storeRef.Name, reconcilationPlan.TuplesToAdd.Count, reconcilationPlan.TuplesToRemove.Count);

        await eventPublisher(
            entity: tupleSet,
            reason: "TupleSetReconcileStarted",
            message: $"Started reconciliation of tuple set. Adding {reconcilationPlan.TuplesToAdd.Count} tuples and removing {reconcilationPlan.TuplesToRemove.Count} tuples. {reconcilationPlan.UnchangedCount} tuples are unchanged.",
            type: EventType.Normal,
            cancellationToken: cancellationToken
        );

        var tuplesWriteResponse = await openFgaService.WriteTuplesAsync(reconcilationPlan, tupleSet.Status.ManagedTupleStates, storeRef.Name, configRef, cancellationToken);
        if (tuplesWriteResponse.IsFullySuccessFul)
        {
            logger.LogInformation("Successfully reconciled tuple set {TupleSetName} for store {StoreRefName}. Added {AddCount} tuples and removed {RemoveCount} tuples.", tupleSet.Name(), storeRef.Name, reconcilationPlan.TuplesToAdd.Count, reconcilationPlan.TuplesToRemove.Count);

            await eventPublisher(
                entity: tupleSet,
                reason: "TupleSetReconcileSuccessful",
                message: $"Tuple set was reconciled successfully. Added {reconcilationPlan.TuplesToAdd.Count} tuples and removed {reconcilationPlan.TuplesToRemove.Count} tuples.",
                type: EventType.Normal,
                cancellationToken: cancellationToken
            );

            return new ReconcileTupleSetResult(true, storeManifest.Status.StoreId, tuplesWriteResponse.TuplesExpectedState);
        }
        else
        {
            foreach (var (tuple, error) in tuplesWriteResponse.FailedTuples)
            {
                logger.LogError("Failed to reconcile tuple set {TupleSetName} for store {StoreRefName}. Tuple '{TupleKey}' failed with error: {ErrorMessage}", tupleSet.Name(), storeRef.Name, $"{tuple.User}|{tuple.Relation}|{tuple.Object}", error);
            }

            await eventPublisher(
                entity: tupleSet,
                reason: "TupleSetReconcileFailed",
                message: $"Tuple set reconciliation encountered errors. {tuplesWriteResponse.FailedTupleCount} tuples failed to reconcile. Check logs for details.",
                type: EventType.Warning,
                cancellationToken: cancellationToken
            );

            return new ReconcileTupleSetResult(false, storeManifest.Status.StoreId, tuplesWriteResponse.TuplesExpectedState);
        }
    }

    public async Task DeleteTupleSetAsync(V1FgaTupleSet tupleSet, CancellationToken cancellationToken = default)
    {
        var storeRef = tupleSet.Spec.StoreRef;
        var storeManifest = await authorizationStoreResolver.ResolveManifestAsync(storeRef.Name, cancellationToken);
        var configRef = storeManifest.Spec.ConnectionConfigRef;

        var reconcilationPlan = TuplesReconcilationPlan.CreateForDelete(tupleSet);
        if (reconcilationPlan.TuplesToRemove.Count == 0)
        {
            return;
        }

        logger.LogInformation("Deleting tuple set {TupleSetName} for store {StoreRefName}. Removing {RemoveCount} tuples.", tupleSet.Name(), storeRef.Name, reconcilationPlan.TuplesToRemove.Count);

        var tuplesWriteResponse = await openFgaService.WriteTuplesAsync(reconcilationPlan, tupleSet.Status.ManagedTupleStates, storeRef.Name, configRef, cancellationToken);
        if (tuplesWriteResponse.IsFullySuccessFul)
        {
            logger.LogInformation("Successfully deleted tuple set {TupleSetName} for store {StoreRefName}. Removed {RemoveCount} tuples.", tupleSet.Name(), storeRef.Name, reconcilationPlan.TuplesToRemove.Count);
        }
        else
        {
            foreach (var (tuple, error) in tuplesWriteResponse.FailedTuples)
            {
                logger.LogError("Failed to delete tuple set {TupleSetName} for store {StoreRefName}. Tuple '{TupleKey}' failed to delete with error: {ErrorMessage}", tupleSet.Name(), storeRef.Name, $"{tuple.User}|{tuple.Relation}|{tuple.Object}", error);
            }
        }
    }
}
