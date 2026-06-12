namespace OpenFga.KubeOps.Models;

public record ReconcileTupleSetResult(bool IsSuccessful, StoreId StoreId, List<Entities.V1FgaTupleSet.V1FgaTupleSetStatus.ManagedFgaTupleState> ManagedTupleStates);
