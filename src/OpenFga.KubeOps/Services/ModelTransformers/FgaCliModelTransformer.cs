using OpenFga.KubeOps.Abstractions;

namespace OpenFga.KubeOps.Services.ModelTransformers;

public class FgaCliModelTransformer : IModelTransformer
{
    public Task<string> TransformDslToJsonAsync(string modelDsl)
    {
        return Task.FromResult("{\"sample\": \"Placeholder\"}");
    }
}
