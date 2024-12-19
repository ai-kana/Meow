using System.Reflection;
using Cysharp.Threading.Tasks;

namespace Meow.Core.Manifest;

public static class ManifestHelper
{
    public static async UniTask CopyToFile(string manifestPath, string filePath)
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        using StreamReader reader = new(assembly.GetManifestResourceStream(manifestPath));
        string content = await reader.ReadToEndAsync();

        using StreamWriter writer = new(File.Open(filePath, FileMode.Create, FileAccess.Write));
        await writer.WriteAsync(content);
    }
}
