using Cysharp.Threading.Tasks;
using Kronstadt.Core.Chat;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands;

[CommandData("reply", "r")]
[CommandSyntax("[<Params: message...>]")]
internal class ReplyCommand : Command
{
    public ReplyCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertArguments(1);

        string message = Context.Form();

        Context.AssertPlayer(out KronstadtPlayer self);
        
        if (self.LastPrivateMessage == null) 
        {
            throw Context.Reply(TranslationList.NoOneToReplyTo);
        }

        if (!KronstadtPlayerManager.TryGetPlayer(self.LastPrivateMessage.Value, out KronstadtPlayer target))
        {
            throw Context.Reply(TranslationList.PlayerNotOnline);
        }
        
        target.LastPrivateMessage = self.SteamID;
        
        KronstadtChat.SendPrivateMessage(self, target, message);
        throw Context.Exit;
    }
}
