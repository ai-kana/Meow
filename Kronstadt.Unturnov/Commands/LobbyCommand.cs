using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using Microsoft.Extensions.Configuration;
using UnityEngine;

namespace Kronstadt.Unturnov.Commands;

[CommandData("lobby", "stuck")]
internal class LobbyCommand : Command
{
    public LobbyCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation LobbyTeleport = new("LobbyTeleport", "Teleported you to lobby");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer caller);
        Context.AssertZoneFlag("stuck");

        IConfigurationSection section = UnturnovPlugin.Configuration.GetRequiredSection("LobbyPosition");
        float x = section.GetValue<float>("X");
        float y = section.GetValue<float>("Y");
        float z = section.GetValue<float>("Z");

        caller.Movement.Teleport(new Vector3(x, y, z));

        throw Context.Reply(LobbyTeleport);
    }
}
