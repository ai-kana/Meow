using Cysharp.Threading.Tasks;
using Kronstadt.Core.Plugins;
using Kronstadt.Unturnov.Doors;

namespace Kronstadt.Unturnov;

public class UnturnovPlugin : Plugin
{
    public override UniTask LoadAsync()
    {
        DoorManager.Load();
        return UniTask.CompletedTask;
    }

    public override UniTask UnloadAsync()
    {
        return UniTask.CompletedTask;
    }
}
