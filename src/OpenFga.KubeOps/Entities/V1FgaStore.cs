using k8s.Models;

using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;

namespace OpenFga.KubeOps.Entities;

[KubernetesEntity(Group = "openfga.sathiyaraman-m.com", ApiVersion = "v1", Kind = "FgaStore")]
[EntityScope(EntityScope.Cluster)]
public sealed class V1FgaStore : CustomKubernetesEntity<V1FgaStore.V1FgaStoreSpec, V1FgaStore.V1FgaStoreStatus>
{
    public class V1FgaStoreSpec
    {
        [Required]
        [Description("Name of the Connection Config to use to connect an OpenFGA instance")]
        public string ConnectionConfigRef { get; set; } = string.Empty;
    }

    public class V1FgaStoreStatus
    {
        [Description("Store ID for the OpenFGA Store")]
        public string StoreId { get; set; } = string.Empty;

        public List<V1Condition> Conditions { get; set; } = [];
    }
}
