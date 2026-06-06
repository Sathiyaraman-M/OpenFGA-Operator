namespace OpenFga.KubeOps.Models;

public record ReconcileTupleSetResult(bool IsSuccessful, List<Entities.V1TupleSet.V1TupleSetStatus.ManagedTupleState> ManagedTupleStates);
