using Cysharp.Threading.Tasks;
using SDG.Framework.Modules;
using SDG.Unturned;
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
            UnturnedLog.exception(ex);
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
