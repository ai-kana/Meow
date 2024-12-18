using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Formatting;
using Meow.Core.Players;
using Meow.Core.Translations;
using Meow.Core.Zones;

namespace Meow.Core.Commands;

[CommandData("zone")]
[CommandSyntax("[<Params: Radius>] [<Params: Flags...>]")]
internal class ZoneCommand : Command
{
    public ZoneCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation ZoneOneFlag = new("ZoneOneFlag", "Zones must have at least one flag");
    private static readonly Translation ZoneCreated = new("ZoneCreated", "Created zone with radius {0} and flags {1}");
    public override async UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer caller);
        Context.AssertOnDuty();
        Context.AssertPermission("zone");
        Context.AssertArguments(2);

        Zone zone = new();
        zone.Center = caller.Movement.Position;
        zone.Radius = Context.Parse<float>();

        Context.MoveNext();
        zone.Flags = Context.Form().Split(' ');
        if (zone.Flags.Length == 0)
        {
            throw Context.Reply(ZoneOneFlag);
        }

        await UniTask.Yield();
        ZoneManager.AddZone(zone);

        throw Context.Reply(ZoneCreated, zone.Radius, Formatter.FormatList(zone.Flags, ", "));
    }
}
