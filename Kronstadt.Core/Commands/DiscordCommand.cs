using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;

namespace Kronstadt.Core.Commands;

[CommandData("discord")]
internal class DiscordCommand : Command
{
    public DiscordCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer caller);

        string? link = KronstadtHost.Configuration.GetValue<string>("DiscordInviteLink");
        if (link == null)
        {
            throw Context.Reply("Discord Link has not been setup!");
        }

        caller.Player.sendBrowserRequest("Click to join our discord!", link);

        throw Context.Exit;
    }
}
