using Cysharp.Threading.Tasks;
using Meow.Core.Chat;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands;

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

        Context.AssertPlayer(out MeowPlayer self);
        
        if (self.LastPrivateMessage == null) 
        {
            throw Context.Reply(TranslationList.NoOneToReplyTo);
        }

        if (!MeowPlayerManager.TryGetPlayer(self.LastPrivateMessage.Value, out MeowPlayer target))
        {
            throw Context.Reply(TranslationList.PlayerNotOnline);
        }
        
        target.LastPrivateMessage = self.SteamID;
        
        MeowChat.SendPrivateMessage(self, target, message);
        throw Context.Exit;
    }
}
