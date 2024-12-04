using Cysharp.Threading.Tasks;

namespace Kronstadt.Core.Plugins;

internal interface IPlugin
{
    public string WorkingDirectory {get;}
    public UniTask LoadAsync();
    public UniTask UnloadAsync();
    internal void SetWorkingDirectory(string dir);
}
