using OpenFga.KubeOps.Abstractions;
using System.Diagnostics;

namespace OpenFga.KubeOps.Services.ModelTransformers;

public sealed class FgaCliModelTransformer : IModelTransformer
{
    public async Task<string> TransformDslToJsonAsync(string modelDsl)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "/usr/local/bin/fga",
            Arguments = $"model transform \"{modelDsl}\" --input-format=fga --output-format=json",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new ModelTransformationFailedException(error);
        }

        return output;
    }
}
