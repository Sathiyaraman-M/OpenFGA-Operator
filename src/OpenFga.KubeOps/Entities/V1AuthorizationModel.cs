using k8s.Models;

using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;
using OpenFga.KubeOps.Entities.Shared;

namespace OpenFga.KubeOps.Entities;

[KubernetesEntity(Group = "openfga.sathiyaraman-m.com", ApiVersion = "v1alpha", Kind = "AuthorizationModel")]
[EntityScope(EntityScope.Cluster)]
public sealed class V1AuthorizationModel : CustomKubernetesEntity<V1AuthorizationModel.V1AuthorizationModelSpec, V1AuthorizationModel.V1AuthorizationModelStatus>
{
    public class V1AuthorizationModelSpec
    {
        public ConnectionConfigReference ConnectionConfigRef { get; set; } = new();

        [Required]
        [Description("FGA Model Content in JSON format")]
        public string ModelJson { get; set; } = string.Empty;

        public AuthorizationStoreReference StoreRef { get; set; } = new();
    }

    public class V1AuthorizationModelStatus
    {
        [Description("Model ID for the current OpenFGA Authorization Model")]
        public string ModelId { get; set; } = string.Empty;

        [Description("SHA256 Hash for the current OpenFGA Authorization Model")]
        public string ObservedModelHash { get; set; } = string.Empty;

        public List<V1Condition> Conditions { get; set; } = [];
    }
}
