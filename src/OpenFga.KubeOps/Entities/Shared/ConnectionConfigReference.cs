using KubeOps.Abstractions.Entities.Attributes;

namespace OpenFga.KubeOps.Entities.Shared;

public sealed class ConnectionConfigReference
{
    [Required]
    [Description("Name of the Connection Config to use to connect an OpenFGA instance")]
    public string Name { get; set; } = string.Empty;
}