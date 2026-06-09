using k8s.Models;

using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;
using OpenFga.KubeOps.Entities.Shared;

namespace OpenFga.KubeOps.Entities;

[KubernetesEntity(Group = "openfga.sathiyaraman-m.com", ApiVersion = "v1", Kind = "FgaTupleSet")]
[EntityScope(EntityScope.Cluster)]
public sealed class V1FgaTupleSet : CustomKubernetesEntity<V1FgaTupleSet.V1FgaTupleSetSpec, V1FgaTupleSet.V1FgaTupleSetStatus>
{
    public class V1FgaTupleSetSpec
    {
        public ConnectionConfigReference ConnectionConfigRef { get; set; } = new();

        public AuthorizationStoreReference StoreRef { get; set; } = new();

        public List<V1FgaTuple> Tuples { get; set; } = [];

        public class V1FgaTuple
        {
            [Required]
            public string User { get; set; } = string.Empty;

            [Required]
            public string Relation { get; set; } = string.Empty;

            [Required]
            public string Object { get; set; } = string.Empty;
        }
    }

    public class V1FgaTupleSetStatus
    {
        [Description("List of Tuples managed by K8s")]
        public List<ManagedFgaTupleState> ManagedTupleStates { get; set; } = [];

        public List<V1Condition> Conditions { get; set; } = [];

        public class ManagedFgaTupleState
        {
            [Description("SHA256 Hash for the tuple")]
            public string Hash { get; set; } = string.Empty;

            public string User { get; set; } = string.Empty;

            public string Relation { get; set; } = string.Empty;

            public string Object { get; set; } = string.Empty;
        }
    }
}
