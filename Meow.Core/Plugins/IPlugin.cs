using Cysharp.Threading.Tasks;

namespace Meow.Core.Plugins;

internal interface IPlugin
{
    public string WorkingDirectory {get;}
    public UniTask LoadAsync();
    public UniTask UnloadAsync();
    internal void SetWorkingDirectory(string dir);
}
