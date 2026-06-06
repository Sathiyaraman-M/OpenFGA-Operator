using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using OpenFga.KubeOps.Entities;
using OpenFga.KubeOps.Extensions;
using OpenFga.KubeOps.Services.Resolvers;
using OpenFga.Sdk.Client.Model;
using System.Security.Cryptography;
using System.Text;

namespace OpenFga.KubeOps.Services;

public class TupleSetService(OpenFgaClientFactory openFgaClientFactory, IKubernetesClient kubernetesClient, AuthorizationStoreResolver authorizationStoreResolver, ILogger<TupleSetService> logger)
{
    public async Task<bool> ReconcileTupleSetAsync(V1TupleSet tupleSet, CancellationToken cancellationToken = default)
    {
        var configRef = tupleSet.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.CreateAsync(configRef.Name, cancellationToken);

        var storeRef = tupleSet.Spec.StoreRef;
        var storeId = await authorizationStoreResolver.ResolveAsync(storeRef.Name, cancellationToken);

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

        if (tuplesToAdd.Count == 0 && tuplesToRemove.Count == 0)
        {
            return true;
        }

        var clientWriteRequest = new ClientWriteRequest(writes: tuplesToAdd, deletes: tuplesToRemove);
        var clientWriteOptions = new ClientWriteOptions() { StoreId = storeId };

        logger.LogInformation("Reconciling tuple set {TupleSetName} for store {StoreId}. Adding {AddCount} tuples and removing {RemoveCount} tuples.", tupleSet.Name(), storeId, tuplesToAdd.Count, tuplesToRemove.Count);

        var clientWriteResponse = await openFgaClient.Write(clientWriteRequest, clientWriteOptions, cancellationToken);

        var fullySuccessful = clientWriteResponse.Writes.Count(x => x.Status == ClientWriteStatus.SUCCESS) == tuplesToAdd.Count
                               && clientWriteResponse.Deletes.Count(x => x.Status == ClientWriteStatus.SUCCESS) == tuplesToRemove.Count;

        if (fullySuccessful)
        {
            logger.LogInformation("Successfully reconciled tuple set {TupleSetName} for store {StoreId}. Added {AddCount} tuples and removed {RemoveCount} tuples.", tupleSet.Name(), storeId, tuplesToAdd.Count, tuplesToRemove.Count);

            tupleSet.Status.Conditions.SetCondition(
                type: "Ready",
                status: "True",
                reason: "TupleSetReconcileSuccessful",
                message: $"Tuple set was reconciled. Added {clientWriteResponse.Writes.Count(x => x.Status == ClientWriteStatus.SUCCESS)} tuples and removed {clientWriteResponse.Deletes.Count(x => x.Status == ClientWriteStatus.SUCCESS)} tuples."
            );

            tupleSet.Status.ManagedTupleStates = [.. desiredStates.Values];
        }
        else
        {
            var failedTuplesWithError = clientWriteResponse.Writes.Where(x => x.Status == ClientWriteStatus.FAILURE)
                .Select(x => (Tuple: x.TupleKey, x.Error))
                .Concat(clientWriteResponse.Deletes.Where(x => x.Status == ClientWriteStatus.FAILURE)
                    .Select(x => (Tuple: x.TupleKey, x.Error)));

            foreach (var (tuple, error) in failedTuplesWithError)
            {
                logger.LogError("Failed to reconcile tuple set {TupleSetName} for store {StoreId}. Tuple '{TupleKey}' failed with error: {ErrorMessage}", tupleSet.Name(), storeId, $"{tuple.User}|{tuple.Relation}|{tuple.Object}", error);
            }

            tupleSet.Status.Conditions.SetCondition(
                type: "Ready",
                status: "False",
                reason: "TupleSetReconcilePartiallySuccessful",
                message: $"Tuple set reconciliation encountered errors. {failedTuplesWithError.Count()} tuples failed to reconcile. Check logs for details."
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

            tupleSet.Status.ManagedTupleStates = [.. managedTupleStatesForExistingTuples, .. managedTupleStatesForSuccessfulNewTuples, .. managedTupleStatesForFailedDeletedTuples];
        }

        tupleSet.Status.StoreId = storeId;

        await kubernetesClient.UpdateStatusAsync(tupleSet, cancellationToken);

        return fullySuccessful;
    }

    public async Task DeleteTupleSetAsync(V1TupleSet tupleSet, CancellationToken cancellationToken = default)
    {
        var configRef = tupleSet.Spec.ConnectionConfigRef;
        using var openFgaClient = await openFgaClientFactory.CreateAsync(configRef.Name, cancellationToken);

        var storeId = tupleSet.Status.StoreId;

        var tuplesToRemove =
            tupleSet.Status.ManagedTupleStates
                .Select(MapToClientTupleKeyWithoutCondition)
                .ToList();

        if (tuplesToRemove.Count == 0)
        {
            return;
        }

        var clientWriteRequest = new ClientWriteRequest(writes: [], deletes: tuplesToRemove);
        var clientWriteOptions = new ClientWriteOptions() { StoreId = storeId };

        logger.LogInformation("Deleting tuple set {TupleSetName} for store {StoreId}. Removing {RemoveCount} tuples.", tupleSet.Name(), storeId, tuplesToRemove.Count);

        var clientWriteResponse = await openFgaClient.Write(clientWriteRequest, clientWriteOptions, cancellationToken);

        if (clientWriteResponse.Deletes.All(x => x.Status == ClientWriteStatus.SUCCESS))
        {
            logger.LogInformation("Successfully deleted tuple set {TupleSetName} for store {StoreId}. Removed {RemoveCount} tuples.", tupleSet.Name(), storeId, tuplesToRemove.Count);
        }
        else
        {
            var failedTuplesWithError = clientWriteResponse.Deletes.Where(x => x.Status == ClientWriteStatus.FAILURE)
                .Select(x => (Tuple: x.TupleKey, x.Error));

            foreach (var (tuple, error) in failedTuplesWithError)
            {
                logger.LogError("Failed to delete tuple set {TupleSetName} for store {StoreId}. Tuple '{TupleKey}' failed to delete with error: {ErrorMessage}", tupleSet.Name(), storeId, $"{tuple.User}|{tuple.Relation}|{tuple.Object}", error);
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
