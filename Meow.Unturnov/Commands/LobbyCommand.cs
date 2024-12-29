using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;
using Microsoft.Extensions.Configuration;
using UnityEngine;

namespace Meow.Unturnov.Commands;

[CommandData("lobby", "stuck")]
internal class LobbyCommand : Command
{
    public LobbyCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation LobbyTeleport = new("LobbyTeleport");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);
        Context.AssertZoneFlag("stuck");

        IConfigurationSection section = UnturnovPlugin.Configuration.GetRequiredSection("LobbyPosition");
        float x = section.GetValue<float>("X");
        float y = section.GetValue<float>("Y");
        float z = section.GetValue<float>("Z");

        caller.Teleport(new Vector3(x, y, z));

        throw Context.Reply(LobbyTeleport);
    }
}
