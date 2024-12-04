using Cysharp.Threading.Tasks;
using SDG.Unturned;
using UnityEngine;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Extensions;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using Command = Kronstadt.Core.Commands.Framework.Command;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("teleport", "tp")]
[CommandSyntax("<[xyz | player,p | location,loc,l | waypoint,wp | here,h]>")]
internal class TeleportCommand : Command
{
    public TeleportCommand(CommandContext context) : base(context)
    {
    }
    
    private bool TryFindLocation(string name, out LocationDevkitNode? node)
    {
        IEnumerable<LocationDevkitNode> nodes = LocationDevkitNodeSystem.Get().GetAllNodes();
        bool Predicate(LocationDevkitNode n) => n.locationName.Contains(name, StringComparison.OrdinalIgnoreCase);
        
        if(nodes.Any(Predicate))
        {
            node = nodes.First(Predicate);
            return true;
        }

        node = null;
        return false;
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("teleport");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        if (Context.HasExactArguments(1))
        {
            Context.AssertPlayer(out KronstadtPlayer self);

            if (TryFindLocation(Context.Current, out LocationDevkitNode? node))
            {
                self.Movement.Teleport(node!.inspectablePosition);
                throw Context.Reply(TranslationList.TeleportedToOther, node.locationName);
            }
            
            KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
            self.Movement.Teleport(player);
            throw Context.Reply(TranslationList.TeleportedToOther, player.Name);
        }

        if (Context.HasExactArguments(2))
        {
            KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
            Context.MoveNext();
            
            if (TryFindLocation(Context.Current, out LocationDevkitNode? node))
            {
                player.Movement.Teleport(node!.inspectablePosition);
                throw Context.Reply(TranslationList.TeleportedOtherToOther, player.Name, node.locationName);
            }
            
            KronstadtPlayer target = Context.Parse<KronstadtPlayer>();

            target.Movement.Teleport(player);
            throw Context.Reply(TranslationList.TeleportedOtherToOther, player.Name, target.Name);
        }

        if (Context.HasExactArguments(3))
        {
            Context.AssertPlayer(out KronstadtPlayer caller);

            Vector3 position = Context.Parse<Vector3>();
            caller.Movement.Teleport(position);
            throw Context.Reply(TranslationList.TeleportingToXYZ, position.x, position.y, position.z);
        }

        {
            KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
            Context.MoveNext();
            Vector3 position = Context.Parse<Vector3>();
            player.Movement.Teleport(position);
            throw Context.Reply(TranslationList.TeleportingOtherToXYZ, player.Name, position.x, position.y, position.z);
        }
    }
}

[CommandParent(typeof(TeleportCommand))]
[CommandData("waypoint", "wp")]
internal class TeleportWaypointCommand : Command
{
    public TeleportWaypointCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("teleport");
        Context.AssertOnDuty();
        Context.AssertPlayer(out KronstadtPlayer self);
        
        if (!self.Quests.TryGetMarkerPosition(out Vector3 position))
        {
            throw Context.Reply(TranslationList.NoWaypoint);
        }
        
        self.Movement.Teleport(position);
        throw Context.Reply(TranslationList.TeleportingToWaypoint);
    }
}

[CommandParent(typeof(TeleportCommand))]
[CommandData("here", "h")]
[CommandSyntax("<[player]>")]
internal class TeleportHereCommand : Command
{
    public TeleportHereCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("teleport");
        Context.AssertOnDuty();
        Context.AssertArguments(1);
        Context.AssertPlayer(out KronstadtPlayer self);

        KronstadtPlayer toTp = Context.Parse<KronstadtPlayer>();
        
        toTp.Movement.Teleport(self);
        throw Context.Reply(TranslationList.TeleportingPlayerHere, toTp.Name);
    }
}
