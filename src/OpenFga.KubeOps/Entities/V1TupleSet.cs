using k8s.Models;

using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;
using OpenFga.KubeOps.Entities.Shared;

namespace OpenFga.KubeOps.Entities;

[KubernetesEntity(Group = "openfga.dev", ApiVersion = "v1alpha", Kind = "TupleSet")]
[EntityScope(EntityScope.Cluster)]
public sealed class V1TupleSet : CustomKubernetesEntity<V1TupleSet.V1TupleSetSpec, V1TupleSet.V1TupleSetStatus>
{
    public class V1TupleSetSpec
    {
        public ConnectionConfigReference ConnectionConfigRef { get; set; } = new();

        public AuthorizationStoreReference StoreRef { get; set; } = new();

        public List<V1Tuple> Tuples { get; set; } = [];

        public class V1Tuple
        {
            [Required]
            public string User { get; set; } = string.Empty;

            [Required]
            public string Relation { get; set; } = string.Empty;

            [Required]
            public string Object { get; set; } = string.Empty;
        }
    }

    public class V1TupleSetStatus
    {
        [Description("List of Tuples managed by K8s")]
        public List<ManagedTupleState> ManagedTupleStates { get; set; } = [];

        public List<V1Condition> Conditions { get; set; } = [];

        public class ManagedTupleState
        {
            [Description("SHA256 Hash for the tuple")]
            public string Hash { get; set; } = string.Empty;

            public string User { get; set; } = string.Empty;

            public string Relation { get; set; } = string.Empty;

            public string Object { get; set; } = string.Empty;
        }
    }
}
