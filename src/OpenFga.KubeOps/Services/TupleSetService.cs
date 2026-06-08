using k8s.Models;
using KubeOps.Abstractions.Events;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Models;
using OpenFga.Sdk.Client.Model;
using System.Security.Cryptography;
using System.Text;

namespace OpenFga.KubeOps.Services;

public class TupleSetService(OpenFgaClientFactory openFgaClientFactory, EventPublisher eventPublisher, ILogger<TupleSetService> logger)
{
    public async Task<ReconcileTupleSetResult> ReconcileTupleSetAsync(V1TupleSet tupleSet, CancellationToken cancellationToken = default)
    {
        var storeRef = tupleSet.Spec.StoreRef;
        var configRef = tupleSet.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.CreateAsync(configRef.Name, storeRef.Name, cancellationToken);

        var desiredStates =
            tupleSet.Spec.Tuples
                .Select(CreateManagedTupleState)
                .DistinctBy(x => x.Hash)
                .ToDictionary(x => x.Hash);

        var existingStates =
            tupleSet.Status.ManagedTupleStates
                .ToDictionary(x => x.Hash);

        var tuplesToAdd =
            desiredStates.Keys
                .Except(existingStates.Keys)
                .Select(hash => desiredStates[hash])
                .Select(MapToClientTupleKey)
                .ToList();

        var tuplesToRemove =
            existingStates.Keys
                .Except(desiredStates.Keys)
                .Select(hash => existingStates[hash])
                .Select(MapToClientTupleKeyWithoutCondition)
                .ToList();

        var unchangedTuplesCount = desiredStates.Keys.Intersect(existingStates.Keys).Count();

        if (tuplesToAdd.Count == 0 && tuplesToRemove.Count == 0)
        {
            return new ReconcileTupleSetResult(true, [.. existingStates.Values]);
        }

        var clientWriteRequest = new ClientWriteRequest(writes: tuplesToAdd, deletes: tuplesToRemove);

        logger.LogInformation("Reconciling tuple set {TupleSetName} for store {StoreRefName}. Adding {AddCount} tuples and removing {RemoveCount} tuples.", tupleSet.Name(), storeRef.Name, tuplesToAdd.Count, tuplesToRemove.Count);

        await eventPublisher(
            entity: tupleSet,
            reason: "TupleSetReconcileStarted",
            message: $"Started reconciliation of tuple set. Adding {tuplesToAdd.Count} tuples and removing {tuplesToRemove.Count} tuples. {unchangedTuplesCount} tuples are unchanged.",
            type: EventType.Normal,
            cancellationToken: cancellationToken
        );

        var clientWriteResponse = await openFgaClient.Write(clientWriteRequest, cancellationToken: cancellationToken);

        var fullySuccessful = clientWriteResponse.Writes.Count(x => x.Status == ClientWriteStatus.SUCCESS) == tuplesToAdd.Count
                               && clientWriteResponse.Deletes.Count(x => x.Status == ClientWriteStatus.SUCCESS) == tuplesToRemove.Count;

        if (fullySuccessful)
        {
            logger.LogInformation("Successfully reconciled tuple set {TupleSetName} for store {StoreRefName}. Added {AddCount} tuples and removed {RemoveCount} tuples.", tupleSet.Name(), storeRef.Name, tuplesToAdd.Count, tuplesToRemove.Count);

            await eventPublisher(
                entity: tupleSet,
                reason: "TupleSetReconcileSuccessful",
                message: $"Tuple set was reconciled successfully. Added {tuplesToAdd.Count} tuples and removed {tuplesToRemove.Count} tuples.",
                type: EventType.Normal,
                cancellationToken: cancellationToken
            );

            return new ReconcileTupleSetResult(true, [.. desiredStates.Values]);
        }
        else
        {
            var failedTuplesWithError = clientWriteResponse.Writes.Where(x => x.Status == ClientWriteStatus.FAILURE)
                .Select(x => (Tuple: x.TupleKey, x.Error))
                .Concat(clientWriteResponse.Deletes.Where(x => x.Status == ClientWriteStatus.FAILURE)
                    .Select(x => (Tuple: x.TupleKey, x.Error)));

            foreach (var (tuple, error) in failedTuplesWithError)
            {
                logger.LogError("Failed to reconcile tuple set {TupleSetName} for store {StoreRefName}. Tuple '{TupleKey}' failed with error: {ErrorMessage}", tupleSet.Name(), storeRef.Name, $"{tuple.User}|{tuple.Relation}|{tuple.Object}", error);
            }

            await eventPublisher(
                entity: tupleSet,
                reason: "TupleSetReconcileFailed",
                message: $"Tuple set reconciliation encountered errors. {failedTuplesWithError.Count()} tuples failed to reconcile. Check logs for details.",
                type: EventType.Warning,
                cancellationToken: cancellationToken
            );

            var managedTupleStatesForExistingTuples = desiredStates.Keys
                .Intersect(existingStates.Keys)
                .Select(hash => existingStates[hash]);

            var managedTupleStatesForSuccessfulNewTuples = clientWriteResponse.Writes.Where(x => x.Status == ClientWriteStatus.SUCCESS)
                .Select(x => x.TupleKey)
                .Select(tupleKey => new V1TupleSet.V1TupleSetStatus.ManagedTupleState()
                {
                    Hash = ComputeHash($"{tupleKey.User}|{tupleKey.Relation}|{tupleKey.Object}"),
                    User = tupleKey.User,
                    Relation = tupleKey.Relation,
                    Object = tupleKey.Object
                })
                .ToList();

            var managedTupleStatesForFailedDeletedTuples = clientWriteResponse.Deletes.Where(x => x.Status == ClientWriteStatus.FAILURE)
                .Select(x => x.TupleKey)
                .Select(tupleKey => new V1TupleSet.V1TupleSetStatus.ManagedTupleState()
                {
                    Hash = ComputeHash($"{tupleKey.User}|{tupleKey.Relation}|{tupleKey.Object}"),
                    User = tupleKey.User,
                    Relation = tupleKey.Relation,
                    Object = tupleKey.Object
                })
                .ToList();

            return new ReconcileTupleSetResult(false, [.. managedTupleStatesForExistingTuples, .. managedTupleStatesForSuccessfulNewTuples, .. managedTupleStatesForFailedDeletedTuples]);
        }
    }

    public async Task DeleteTupleSetAsync(V1TupleSet tupleSet, CancellationToken cancellationToken = default)
    {
        var storeRef = tupleSet.Spec.StoreRef;
        var configRef = tupleSet.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.CreateAsync(configRef.Name, storeRef.Name, cancellationToken);

        var tuplesToRemove =
            tupleSet.Status.ManagedTupleStates
                .Select(MapToClientTupleKeyWithoutCondition)
                .ToList();

        if (tuplesToRemove.Count == 0)
        {
            return;
        }

        var clientWriteRequest = new ClientWriteRequest(writes: [], deletes: tuplesToRemove);

        logger.LogInformation("Deleting tuple set {TupleSetName} for store {StoreRefName}. Removing {RemoveCount} tuples.", tupleSet.Name(), storeRef.Name, tuplesToRemove.Count);

        var clientWriteResponse = await openFgaClient.Write(clientWriteRequest, cancellationToken: cancellationToken);

        if (clientWriteResponse.Deletes.All(x => x.Status == ClientWriteStatus.SUCCESS))
        {
            logger.LogInformation("Successfully deleted tuple set {TupleSetName} for store {StoreRefName}. Removed {RemoveCount} tuples.", tupleSet.Name(), storeRef.Name, tuplesToRemove.Count);
        }
        else
        {
            var failedTuplesWithError = clientWriteResponse.Deletes.Where(x => x.Status == ClientWriteStatus.FAILURE)
                .Select(x => (Tuple: x.TupleKey, x.Error));

            foreach (var (tuple, error) in failedTuplesWithError)
            {
                logger.LogError("Failed to delete tuple set {TupleSetName} for store {StoreRefName}. Tuple '{TupleKey}' failed to delete with error: {ErrorMessage}", tupleSet.Name(), storeRef.Name, $"{tuple.User}|{tuple.Relation}|{tuple.Object}", error);
            }
        }
    }

    private static V1TupleSet.V1TupleSetStatus.ManagedTupleState CreateManagedTupleState(
        V1TupleSet.V1TupleSetSpec.V1Tuple tuple)
    {
        return new()
        {
            Hash = ComputeHash($"{tuple.User}|{tuple.Relation}|{tuple.Object}"),
            User = tuple.User,
            Relation = tuple.Relation,
            Object = tuple.Object
        };
    }

    private static ClientTupleKey MapToClientTupleKey(V1TupleSet.V1TupleSetStatus.ManagedTupleState tupleState)
    {
        return new ClientTupleKey()
        {
            User = tupleState.User,
            Relation = tupleState.Relation,
            Object = tupleState.Object
        };
    }

    private static ClientTupleKeyWithoutCondition MapToClientTupleKeyWithoutCondition(V1TupleSet.V1TupleSetStatus.ManagedTupleState tupleState)
    {
        return new ClientTupleKeyWithoutCondition()
        {
            User = tupleState.User,
            Relation = tupleState.Relation,
            Object = tupleState.Object
        };
    }

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
