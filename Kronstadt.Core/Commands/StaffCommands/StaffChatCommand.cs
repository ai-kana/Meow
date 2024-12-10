using Cysharp.Threading.Tasks;
using Kronstadt.Core.Chat;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;

namespace Kronstadt.Core.Commands;

[CommandData("staffchat", "sc")]
[CommandSyntax("[<Params: message...>]")]
internal class StaffChatCommand : Command
{
    public StaffChatCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer player);
        Context.AssertPermission("staffchat");
        Context.AssertArguments(1);

        string msg = Context.Form();
        KronstadtChat.SendStaffChat(msg, player);
        throw Context.Exit;
    }
}
