using k8s.Models;

using KubeOps.Abstractions.Entities;
using KubeOps.Abstractions.Entities.Attributes;

namespace OpenFga.KubeOps.Entities;

[KubernetesEntity(Group = "openfga.sathiyaraman-m.com", ApiVersion = "v1", Kind = "FgaConnectionConfig")]
[EntityScope(EntityScope.Cluster)]
public sealed class V1FgaConnectionConfig : CustomKubernetesEntity<V1FgaConnectionConfig.V1FgaConnectionConfigSpec>
{
    public class V1FgaConnectionConfigSpec
    {
        [Required]
        [Description("API URL for the OpenFGA Instance")]
        public string ApiUrl { get; set; } = string.Empty;

        public V1SecretReference CredentialRef { get; set; } = new();
    }
}
