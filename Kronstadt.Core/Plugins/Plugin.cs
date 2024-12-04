using Cysharp.Threading.Tasks;

namespace Kronstadt.Core.Plugins;

public abstract class Plugin : IPlugin
{
    public string WorkingDirectory => _WorkingDirectory;
    private string _WorkingDirectory = "";

    public abstract UniTask LoadAsync();

    public abstract UniTask UnloadAsync();

    void IPlugin.SetWorkingDirectory(string dir)
    {
        _WorkingDirectory = dir;
    }
}
