namespace OpenFga.KubeOps.Models;

public record AuthorizationModelId
{
    public string Value { get; init; }

    public AuthorizationModelId(string value) => Value = value;

    public static implicit operator string(AuthorizationModelId authorizationModelId) => authorizationModelId.Value;
    public static implicit operator AuthorizationModelId(string authorizationModelId) => new(authorizationModelId);
}

