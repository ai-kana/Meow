using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Offenses;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("offenses")]
[CommandSyntax("[<Params: player>] [?<Params: bans, warns, mutes>]")]
internal class OffensesCommand : Command
{
    public OffensesCommand(CommandContext context) : base(context)
    {
    }

    public static readonly Translation NoOffenses = new("NoOffenses");
    public static readonly Translation Offense = new("Offense");
    public static readonly Translation OffenseList = new("OffenseList");
    public static readonly Translation OffenseType = new("OffenseType");

    public static readonly Translation Bans = new("Bans");
    public static readonly Translation Mutes = new("Mutes");
    public static readonly Translation Warns = new("Warns");
    

    public override async UniTask ExecuteAsync()
    {
        Context.AssertPermission("offenses");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        MeowPlayer target = Context.Parse<MeowPlayer>();
        
        if (Context.HasExactArguments(1))
        {
            IEnumerable<Offense> offensesI = await OffenseManager.GetOffenses(target.SteamID);
            List<Offense> offenses =  offensesI.Take(5).ToList();
            
            if (offenses.Count == 0)
            {
                throw Context.Reply(NoOffenses, target.Name);
            }
            
            Context.Reply(OffenseList, target.Name);
            foreach (Offense offense in offenses)
            {
                Context.Reply(Offense, offense.Id, offense.Reason);
            }

            throw Context.Exit;
        }
        
        Context.MoveNext();
        string type = Context.Form().ToLower();

        List<Offense> offensesList;
        IEnumerable<Offense> enumerableOffenses;
        Translation offenseType;

        switch (type)
        {
            case "w": 
            case "warn": 
            case "warns": 
                enumerableOffenses = await OffenseManager.GetWarnOffenses(target.SteamID);
                offenseType = Warns;
                break;
            case "b": 
            case "ban": 
            case "bans":
                enumerableOffenses = await OffenseManager.GetBanOffenses(target.SteamID);
                offenseType = Bans;
                break;
            case "m": 
            case "mute": 
            case "mutes":
                enumerableOffenses = await OffenseManager.GetMuteOffenses(target.SteamID);
                offenseType = Mutes;
                break;
            default:
                throw Context.Reply("<[player] [bans,b | warns,w | mutes,m]>");
        }
        
        offensesList = enumerableOffenses.OrderByDescending(w => w.Id).Take(5).ToList();
        
        if(offensesList.Count == 0)
        {
            throw Context.Reply(NoOffenses, target.Name);
        }
        
        Context.Reply(OffenseType, new TranslationPackage(offenseType), target.Name);
        
        offensesList.ForEach(offense => 
            Context.Reply(Offense, offense.Id, offense.Reason)
        );

        throw Context.Exit;
    }
}
