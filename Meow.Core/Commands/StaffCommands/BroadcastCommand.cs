using Cysharp.Threading.Tasks;
using Meow.Core.Chat;
using Meow.Core.Commands.Framework;

namespace Meow.Core.Commands.StaffCommands;

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
        MeowChat.BroadcastMessage(message);
        throw Context.Exit;
    }
}
