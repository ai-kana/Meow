using SDG.Framework.Modules;

namespace Meow.Core;

public sealed class UnturnovModule : IModuleNexus
{
    private MeowHost? _Host;

    public async void initialize()
    {
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
