using Cysharp.Threading.Tasks;
using SDG.Unturned;
using UnityEngine;
using Meow.Core.Commands.Framework;
using Meow.Core.Extensions;
using Meow.Core.Players;
using Meow.Core.Translations;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("teleport", "tp")]
[CommandSyntax("[<Params: xyz, player, location> <Switches: here, waypoint>] [?<Params: player, location>]")]
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
            Context.AssertPlayer(out MeowPlayer self);

            if (TryFindLocation(Context.Current, out LocationDevkitNode? node))
            {
                self.Movement.Teleport(node!.inspectablePosition);
                throw Context.Reply(TranslationList.TeleportedToOther, node.locationName);
            }
            
            MeowPlayer player = Context.Parse<MeowPlayer>();
            self.Movement.Teleport(player);
            throw Context.Reply(TranslationList.TeleportedToOther, player.Name);
        }

        if (Context.HasExactArguments(2))
        {
            MeowPlayer player = Context.Parse<MeowPlayer>();
            Context.MoveNext();
            
            if (TryFindLocation(Context.Current, out LocationDevkitNode? node))
            {
                player.Movement.Teleport(node!.inspectablePosition);
                throw Context.Reply(TranslationList.TeleportedOtherToOther, player.Name, node.locationName);
            }
            
            MeowPlayer target = Context.Parse<MeowPlayer>();

            target.Movement.Teleport(player);
            throw Context.Reply(TranslationList.TeleportedOtherToOther, player.Name, target.Name);
        }

        if (Context.HasExactArguments(3))
        {
            Context.AssertPlayer(out MeowPlayer caller);

            Vector3 position = Context.Parse<Vector3>();
            caller.Movement.Teleport(position);
            throw Context.Reply(TranslationList.TeleportingToXYZ, position.x, position.y, position.z);
        }

        {
            MeowPlayer player = Context.Parse<MeowPlayer>();
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
        Context.AssertPlayer(out MeowPlayer self);
        
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
        Context.AssertPlayer(out MeowPlayer self);

        MeowPlayer toTp = Context.Parse<MeowPlayer>();
        
        toTp.Movement.Teleport(self);
        throw Context.Reply(TranslationList.TeleportingPlayerHere, toTp.Name);
    }
}