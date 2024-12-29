using Cysharp.Threading.Tasks;
using Meow.Core.Chat;
using Meow.Core.Commands.Framework;
using Meow.Core.Extensions;
using Meow.Core.Players;
using Meow.Core.Translations;
using Steamworks;

namespace Meow.Core.Commands;

[CommandData("reply", "r")]
[CommandSyntax("[<Params: message...>]")]
internal class ReplyCommand : Command
{
    public ReplyCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation NoOneToReplyTo = new("NoOneToReplyTo");
    private static readonly Translation PlayerNotOnline = new("PlayerNotOnline");

    public readonly  static Dictionary<MeowPlayer, CSteamID> LastMessage = new();

    public override UniTask ExecuteAsync()
    {
        Context.AssertArguments(1);

        string message = Context.Form();

        Context.AssertPlayer(out MeowPlayer self);
        
        if (!LastMessage.TryGetValue(self, out CSteamID last) || last == CSteamID.Nil)
        {
            throw Context.Reply(NoOneToReplyTo);
        }

        if (!MeowPlayerManager.TryGetPlayer(last, out MeowPlayer target))
        {
            throw Context.Reply(PlayerNotOnline);
        }
        
        LastMessage.AddOrUpdate(target, self.SteamID);
        
        MeowChat.SendPrivateMessage(self, target, message);
        throw Context.Exit;
    }
}
