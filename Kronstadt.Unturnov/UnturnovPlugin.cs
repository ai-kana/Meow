using Cysharp.Threading.Tasks;
using Kronstadt.Core.Plugins;
using Kronstadt.Unturnov.Doors;
using Microsoft.Extensions.Configuration;

namespace Kronstadt.Unturnov;

public class UnturnovPlugin : Plugin
{
    public static IConfiguration Configuration = null!;

    public override UniTask LoadAsync()
    {
        DoorManager.Load();
        ConfigurationBuilder builder = new();
        builder.AddJsonFile(Path.Combine(WorkingDirectory, "Kronstadt.Unturnov.Configuration.json"));
        Configuration = builder.Build();

        return UniTask.CompletedTask;
    }

    public override UniTask UnloadAsync()
    {
        return UniTask.CompletedTask;
    }
}
