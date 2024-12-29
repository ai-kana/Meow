using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Offenses;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("warn")]
[CommandSyntax("[<Switches: add, remove, list>]")]
internal class WarnCommand : Command
{
    public WarnCommand(CommandContext context) : base(context)
    {
    }
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("warn");
        Context.AssertOnDuty();
        
        throw Context.Reply("<add | remove | list>");
    }
}

[CommandParent(typeof(WarnCommand))]
[CommandData("add", "a")]
[CommandSyntax("[<Params: player>] [<Params: reason...>]")]
internal class WarnAddCommand : Command
{
    public WarnAddCommand(CommandContext context) : base(context)
    {
    }
    // Warn
    public static readonly Translation WarnedReason = new("WarnedReason");
    
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("warn.add");
        Context.AssertOnDuty();
        Context.AssertArguments(2);
        Context.AssertPlayer(out MeowPlayer self);
        
        MeowPlayer target = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        string reason = Context.Form();
        
        target.AddWarn(self.SteamID, reason).Forget();
        
        throw Context.Reply(WarnedReason, target.Name, reason);
    }
}

[CommandParent(typeof(WarnCommand))]
[CommandData("remove", "r")]
[CommandSyntax("[<Params: player>]")]
internal class WarnRemoveCommand : Command
{
    public WarnRemoveCommand(CommandContext context) : base(context)
    {
    }
    public static readonly Translation PardonedWarn = new("PardonedWarn");
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("warn.remove");
        Context.AssertOnDuty();
        Context.AssertArguments(1);
        
        int id = Context.Parse<int>();

        OffenseManager.PardonOffense(id).Forget();
        
        throw Context.Reply(PardonedWarn, id);
    }
}

[CommandParent(typeof(WarnCommand))]
[CommandData("list", "l")]
[CommandSyntax("[<Params: player>]")]
internal class WarnListCommand : Command
{
    public WarnListCommand(CommandContext context) : base(context)
    {
    }
    
    public static readonly Translation HasNoWarns = new("HasNoWarns");
    public static readonly Translation WarningListed = new("WarningListed");

    public override async UniTask ExecuteAsync()
    {
        Context.AssertPermission("warn.list");
        Context.AssertOnDuty();
        Context.AssertArguments(1);
        
        MeowPlayer target = Context.Parse<MeowPlayer>();

        IEnumerable<Offense> warns = await target.GetWarns();
        List<Offense> warnsList = warns.Where(w => w.Pardoned == false).ToList();
        
        if (!warnsList.Any())
        {
            throw Context.Reply(HasNoWarns, target.Name);
        }

        List<Offense> lastFiveWarns = warnsList.OrderByDescending(w => w.Issued).Take(5).ToList();
        
        foreach (Offense lastFiveWarn in lastFiveWarns)
        {
            Context.Reply(WarningListed, lastFiveWarn.Id, lastFiveWarn.Issued, lastFiveWarn.Reason);
        }

        throw Context.Exit;
    }
}
