using Cysharp.Threading.Tasks;

namespace Kronstadt.Core.Commands.Framework;

public abstract class Command
{
    public CommandContext Context {get;}
    public abstract UniTask ExecuteAsync();

    protected Command(CommandContext context)
    {
        Context = context;
    }
}
