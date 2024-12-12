using Cysharp.Threading.Tasks;
using Meow.Core.Chat;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;

namespace Meow.Core.Commands;

[CommandData("staffchat", "sc")]
[CommandSyntax("[<Params: message...>]")]
internal class StaffChatCommand : Command
{
    public StaffChatCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer player);
        Context.AssertPermission("staffchat");
        Context.AssertArguments(1);

        string msg = Context.Form();
        MeowChat.SendStaffChat(msg, player);
        throw Context.Exit;
    }
}
