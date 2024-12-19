using Cysharp.Threading.Tasks;
using SDG.Framework.Modules;
using UnityEngine.LowLevel;

namespace Meow.Core;

public sealed class MeowModule : IModuleNexus
{
    private MeowHost? _Host;

    public async void initialize()
    {
        PlayerLoopSystem system = PlayerLoop.GetCurrentPlayerLoop();
        PlayerLoopHelper.Initialize(ref system);

        try
        {
            _Host = new();
            await _Host.LoadAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async void shutdown()
    {
        if (_Host == null) 
        {
            return;
        }

        await _Host.UnloadAsync();
    }
}
