using System.Reflection;
using Cysharp.Threading.Tasks;
using Meow.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Meow.Core.Manifest;

public class ManifestHelper
{
    public static async UniTask CopyToFile(string manifestPath, string filePath)
    {
        ILogger logger = LoggerProvider.CreateLogger<ManifestHelper>();
        Assembly assembly = Assembly.GetCallingAssembly();
        logger.LogDebug($"Loading manifest resource: \"{manifestPath}\"");
        using Stream stream = assembly.GetManifestResourceStream(manifestPath);
        if (stream == null) {
            return;
        }
        using StreamReader reader = new(stream);
        string content = await reader.ReadToEndAsync();

        using StreamWriter writer = new(File.Open(filePath, FileMode.Create, FileAccess.Write));
        await writer.WriteAsync(content);
    }
}
