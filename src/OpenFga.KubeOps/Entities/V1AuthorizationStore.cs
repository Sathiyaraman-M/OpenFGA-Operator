using k8s.Models;

using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;
using OpenFga.KubeOps.Entities.Shared;

namespace OpenFga.KubeOps.Entities;

[KubernetesEntity(Group = "openfga.dev", ApiVersion = "v1alpha", Kind = "AuthorizationStore")]
[EntityScope(EntityScope.Cluster)]
public sealed class V1AuthorizationStore : CustomKubernetesEntity<V1AuthorizationStore.V1AuthorizationStoreSpec, V1AuthorizationStore.V1AuthorizationStoreStatus>
{
    public class V1AuthorizationStoreSpec
    {
        public ConnectionConfigReference ConnectionConfigRef { get; set; } = new();
    }

    public class V1AuthorizationStoreStatus
    {
        [Description("Store ID for the OpenFGA Store")]
        public string StoreId { get; set; } = string.Empty;

        public List<V1Condition> Conditions { get; set; } = [];
    }
}
