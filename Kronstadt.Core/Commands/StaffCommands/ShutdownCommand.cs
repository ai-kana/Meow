using Cysharp.Threading.Tasks;
using Kronstadt.Core.Chat;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("shutdown", "q")]
[CommandSyntax("<[delay] | cancel>?")]
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

        KronstadtChat.BroadcastMessage(TranslationList.ShutdownCancelled);
        throw Context.Exit;
    }
}
