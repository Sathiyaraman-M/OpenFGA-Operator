using k8s.Models;

using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;

namespace OpenFga.KubeOps.Entities;

[KubernetesEntity(Group = "openfga.sathiyaraman-m.com", ApiVersion = "v1", Kind = "FgaAuthorizationModel")]
[EntityScope(EntityScope.Cluster)]
public sealed class V1FgaAuthorizationModel : CustomKubernetesEntity<V1FgaAuthorizationModel.V1FgaAuthorizationModelSpec, V1FgaAuthorizationModel.V1FgaAuthorizationModelStatus>
{
    public class V1FgaAuthorizationModelSpec
    {
        [Required]
        [Description("FGA Model Content in DSL format")]
        public string ModelDsl { get; set; } = string.Empty;

        [Required]
        [Description("Name of the OpenFGA Store to which this Authorization Model belongs")]
        public string StoreRef { get; set; } = string.Empty;
    }

    public class V1FgaAuthorizationModelStatus
    {
        [Description("Store ID for the OpenFGA Store associated with the Authorization Model")]
        public string StoreId { get; set; } = string.Empty;

        [Description("Model ID for the current OpenFGA Authorization Model")]
        public string ModelId { get; set; } = string.Empty;

        [Description("SHA256 Hash for the current OpenFGA Authorization Model")]
        public string ObservedModelHash { get; set; } = string.Empty;

        public List<V1Condition> Conditions { get; set; } = [];
    }
}
