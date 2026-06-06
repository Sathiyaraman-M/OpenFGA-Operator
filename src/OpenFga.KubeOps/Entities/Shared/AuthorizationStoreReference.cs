using KubeOps.Abstractions.Entities.Attributes;

namespace OpenFga.KubeOps.Entities.Shared;

public sealed class AuthorizationStoreReference
{
    [Required]
    [Description("Name of the Target OpenFGA Store")]
    public string Name { get; set; } = string.Empty;
}
