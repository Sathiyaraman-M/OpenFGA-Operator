namespace OpenFga.KubeOps.Models;

public record StoreId
{
    public string Value { get; init; }

    public StoreId(string value) => Value = value;

    public static implicit operator string(StoreId storeId) => storeId.Value;
    public static implicit operator StoreId(string storeId) => new(storeId);
}
