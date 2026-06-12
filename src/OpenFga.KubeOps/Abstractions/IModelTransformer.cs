namespace OpenFga.KubeOps.Abstractions;

public interface IModelTransformer
{
    Task<string> TransformDslToJsonAsync(string modelDsl);
}