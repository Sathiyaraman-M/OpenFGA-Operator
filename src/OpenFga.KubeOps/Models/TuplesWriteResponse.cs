using OpenFga.KubeOps.Entities;
using OpenFga.Sdk.Client.Model;
using OpenFga.Sdk.Model;
using System.Security.Cryptography;
using System.Text;

namespace OpenFga.KubeOps.Models;

public class TuplesWriteResponse
{
    public bool IsFullySuccessFul { get; set; }

    public List<(TupleKey, Exception?)> FailedTuples { get; }

    public int FailedTupleCount => FailedTuples.Count;

    public List<V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState> TuplesExpectedState { get; }

    private TuplesWriteResponse(bool isfullySuccessful, List<(TupleKey, Exception?)> failedTuples, List<V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState> tuplesExpectedState)
    {
        FailedTuples = failedTuples;
        TuplesExpectedState = tuplesExpectedState;
        IsFullySuccessFul = isfullySuccessful;
    }

    public static TuplesWriteResponse Create(ClientWriteResponse clientWriteResponse, TuplesReconcilationPlan originalPlan, IReadOnlyList<V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState> existingTupleStates)
    {
        var fullySuccessful = clientWriteResponse.Writes.Count(x => x.Status == ClientWriteStatus.SUCCESS) == originalPlan.TuplesToAdd.Count
                               && clientWriteResponse.Deletes.Count(x => x.Status == ClientWriteStatus.SUCCESS) == originalPlan.TuplesToRemove.Count;

        if (fullySuccessful)
        {
            return new TuplesWriteResponse(true, [], [.. originalPlan.DesiredStates]);
        }

        var failedTuplesWithError = clientWriteResponse.Writes.Where(x => x.Status == ClientWriteStatus.FAILURE)
            .Select(x => (x.TupleKey, x.Error))
            .Concat(clientWriteResponse.Deletes.Where(x => x.Status == ClientWriteStatus.FAILURE)
                .Select(x => (x.TupleKey, x.Error)))
            .ToList();

        var existingTupleStatesDict = existingTupleStates.DistinctBy(x => x.Hash).ToDictionary(x => x.Hash);
        var desiredTupleStatesDict = originalPlan.DesiredStates.DistinctBy(x => x.Hash).ToDictionary(x => x.Hash);

        var managedTuplesStateForExistingTuples = existingTupleStatesDict.Keys
            .Intersect(desiredTupleStatesDict.Keys)
            .Select(hash => existingTupleStatesDict[hash])
            .ToList();

        var managedTuplesStateForSuccessfullyAddedTuples = clientWriteResponse.Writes
            .Where(x => x.Status == ClientWriteStatus.SUCCESS)
            .Select(x => x.TupleKey)
            .Select(MapToManagedTupleState)
            .ToList();

        var managedTuplesStateForFailedRemovedTuples = clientWriteResponse.Deletes
            .Where(x => x.Status == ClientWriteStatus.FAILURE)
            .Select(x => x.TupleKey)
            .Select(MapToManagedTupleState)
            .ToList();

        var expectedStateForTuples = managedTuplesStateForExistingTuples
            .Concat(managedTuplesStateForSuccessfullyAddedTuples)
            .Concat(managedTuplesStateForFailedRemovedTuples)
            .DistinctBy(x => x.Hash)
            .ToList();

        return new TuplesWriteResponse(false, failedTuplesWithError, expectedStateForTuples);
    }

    private static V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState MapToManagedTupleState(TupleKey tupleKey)
    {
        return new V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState
        {
            Hash = ComputeHash($"{tupleKey.User}|{tupleKey.Relation}|{tupleKey.Object}"),
            User = tupleKey.User,
            Relation = tupleKey.Relation,
            Object = tupleKey.Object
        };
    }

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
