using Cysharp.Threading.Tasks;
using Kronstadt.Core.Chat;
using Kronstadt.Core.Commands.Framework;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("broadcast", "say")]
[CommandSyntax("[<Params: message...>]")]
internal class BroadcastCommand : Command
{
    public BroadcastCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("broadcast");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        string message = Context.Form();
        KronstadtChat.BroadcastMessage(message);
        throw Context.Exit;
    }
}
