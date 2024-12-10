using Cysharp.Threading.Tasks;
using Kronstadt.Core.Chat;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands;

[CommandData("pm", "dm", "msg")]
[CommandSyntax("[<Params: player>] [<Params: message>]")]
internal class PrivateMessageCommand : Command
{
    public PrivateMessageCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertArguments(2);
        Context.AssertPlayer(out KronstadtPlayer self);
        
        KronstadtPlayer target = Context.Parse<KronstadtPlayer>();
        Context.MoveNext();
        string message = Context.Form();
        
        if (target.SteamID == self.SteamID)
        {
            throw Context.Reply(TranslationList.PrivateMessageSelf);
        }
        
        target.LastPrivateMessage = self.SteamID;

        KronstadtChat.SendPrivateMessage(self, target, message);
        throw Context.Exit;
    }
}
