using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;

namespace Meow.Core.Commands;

[CommandData("discord")]
internal class DiscordCommand : Command
{
    public DiscordCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);

        string? link = MeowHost.Configuration.GetValue<string>("DiscordInviteLink");
        if (link == null)
        {
            throw Context.Reply("Discord Link has not been setup!");
        }

        caller.Player.sendBrowserRequest("Click to join our discord!", link);

        throw Context.Exit;
    }
}
