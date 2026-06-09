namespace OpenFga.KubeOps.Models;

public record ReconcileTupleSetResult(bool IsSuccessful, List<Entities.V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState> ManagedTupleStates);
