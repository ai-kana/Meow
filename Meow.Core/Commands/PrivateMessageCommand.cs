using Cysharp.Threading.Tasks;
using Meow.Core.Chat;
using Meow.Core.Commands.Framework;
using Meow.Core.Extensions;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands;

[CommandData("pm", "dm", "msg")]
[CommandSyntax("[<Params: player>] [<Params: message>]")]
internal class PrivateMessageCommand : Command
{
    public PrivateMessageCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation PrivateMessageSelf = new("PrivateMessageSelf");
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertArguments(2);
        Context.AssertPlayer(out MeowPlayer self);
        
        MeowPlayer target = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        string message = Context.Form();
        
        if (target.SteamID == self.SteamID)
        {
            throw Context.Reply(PrivateMessageSelf);
        }
        
        ReplyCommand.LastMessage.AddOrUpdate(target, self.SteamID);

        MeowChat.SendPrivateMessage(self, target, message);
        throw Context.Exit;
    }
}
