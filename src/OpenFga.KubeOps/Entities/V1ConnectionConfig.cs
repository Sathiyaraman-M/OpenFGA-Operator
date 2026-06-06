using k8s.Models;

using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;

namespace OpenFga.KubeOps.Entities;

[KubernetesEntity(Group = "openfga.dev", ApiVersion = "v1alpha", Kind = "ConnectionConfig")]
public sealed class V1ConnectionConfig : CustomKubernetesEntity<V1ConnectionConfig.V1ConnectionConfigSpec>
{
    public class V1ConnectionConfigSpec
    {
        [Required]
        [Description("API URL for the OpenFGA Instance")]
        public string ApiUrl { get; set; } = string.Empty;

        public V1SecretReference CredentialRef { get; set; } = new();
    }
}
