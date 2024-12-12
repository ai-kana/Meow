using Cysharp.Threading.Tasks;
using Meow.Core.Chat;
using Meow.Core.Commands.Framework;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("shutdown", "q")]
[CommandSyntax("[<Params: delay> <Switch: cancel>]")]
internal class ShutdownCommand : Command
{
    public ShutdownCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("shutdown");
        Context.AssertOnDuty();

        uint delay = 1;
        if (Context.HasArguments(1))
        {
            delay = Context.Parse<uint>();
        }

        ServerManager.QueueShutdown(delay);
        throw Context.Exit;
    }
}

[CommandParent(typeof(ShutdownCommand))]
[CommandData("cancel")]
internal class ShutdownCancelCommand : Command
{
    public ShutdownCancelCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("shutdown");
        Context.AssertOnDuty();

        if (!ServerManager.CancelShutdown())
        {
            throw Context.Reply(TranslationList.ShutdownNotActive);
        }

        MeowChat.BroadcastMessage(TranslationList.ShutdownCancelled);
        throw Context.Exit;
    }
}
