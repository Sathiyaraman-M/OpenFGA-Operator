using OpenFga.KubeOps.Entities;
using OpenFga.Sdk.Client.Model;
using System.Security.Cryptography;
using System.Text;

namespace OpenFga.KubeOps.Models;

public class TuplesReconcilationPlan
{
    public IReadOnlyList<V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState> ExistingStates { get; } = [];
    public IReadOnlyList<V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState> DesiredStates { get; } = [];

    public IReadOnlyList<ClientTupleKey> TuplesToAdd { get; } = [];
    public IReadOnlyList<ClientTupleKeyWithoutCondition> TuplesToRemove { get; } = [];

    public int UnchangedCount { get; }

    private TuplesReconcilationPlan(
        IReadOnlyList<V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState> existingStates,
        IReadOnlyList<V1FgaTupleSet.V1FgaTupleSetSpec.V1FgaTuple> desiredStates)
    {
        ExistingStates = [.. existingStates.DistinctBy(x => x.Hash)];
        DesiredStates = [.. desiredStates.Select(CreateManagedTupleState).DistinctBy(x => x.Hash)];

        var existingStatesDict = existingStates.ToDictionary(x => x.Hash);
        var desiredStatesDict = DesiredStates.ToDictionary(x => x.Hash);

        TuplesToAdd = [.. desiredStatesDict.Keys
            .Except(existingStatesDict.Keys)
            .Select(hash => desiredStatesDict[hash])
            .Select(MapToClientTupleKey)];

        TuplesToRemove = [.. existingStatesDict.Keys
            .Except(desiredStatesDict.Keys)
            .Select(hash => existingStatesDict[hash])
            .Select(MapToClientTupleKeyWithoutCondition)];

        UnchangedCount = desiredStatesDict.Keys.Intersect(existingStatesDict.Keys).Count();
    }

    public static TuplesReconcilationPlan CreateForWrite(V1FgaTupleSet tupleSet) => new(tupleSet.Status.ManagedTupleStates, tupleSet.Spec.Tuples);

    public static TuplesReconcilationPlan CreateForDelete(V1FgaTupleSet tupleSet)
    {
        var existingStates = tupleSet.Status.ManagedTupleStates;
        return new TuplesReconcilationPlan(existingStates, []);
    }

    private static V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState CreateManagedTupleState(
        V1FgaTupleSet.V1FgaTupleSetSpec.V1FgaTuple tuple) => new()
        {
            Hash = ComputeHash($"{tuple.User}|{tuple.Relation}|{tuple.Object}"),
            User = tuple.User,
            Relation = tuple.Relation,
            Object = tuple.Object
        };

    private static ClientTupleKey MapToClientTupleKey(V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState tupleState) => new()
    {
        User = tupleState.User,
        Relation = tupleState.Relation,
        Object = tupleState.Object
    };

    private static ClientTupleKeyWithoutCondition MapToClientTupleKeyWithoutCondition(V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState tupleState) => new()
    {
        User = tupleState.User,
        Relation = tupleState.Relation,
        Object = tupleState.Object
    };

    private static string ComputeHash(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes);
    }
}
