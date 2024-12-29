using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Stats;
using Meow.Core.Translations;
using Steamworks;
using Session = Meow.Core.Players.PlayerState.Session;
using PlayerStats = Meow.Core.Players.MeowPlayer.PlayerStats;

namespace Meow.Core.Commands;

[CommandData("stats")]
[CommandSyntax("[<Params: player?> <Switches: life, session>]")]
internal class StatsCommand : Command
{
    public StatsCommand(CommandContext context) : base(context)
    {
    }

    public static readonly Translation FailedToGetStats = new("FailedToGetStats");
    private static readonly Translation PlayerStats = new("PlayerStats");

    public override async UniTask ExecuteAsync()
    {
        MeowPlayer player = default;
        string name;
        CSteamID target;
        if (Context.HasExactArguments(0))
        {
            Context.AssertPlayer(out player);
            target = player.SteamID;
            name = player.Name;
        }
        else if (Context.TryParse<MeowPlayer>(out player))
        {
            name = player.Name;
            target = player.SteamID;
        }
        else
        {
            target = Context.Parse<CSteamID>();
            name = target.ToString();
        }

        PlayerStats? stats = player.Stats ?? await StatsManager.GetStats(target);
        if (stats == null)
        {
            throw Context.Reply(FailedToGetStats, name);
        }

        uint fish = stats.Fish + player.ServerSession.Fish;
        uint kills = stats.Kills + player.ServerSession.Kills;
        uint deaths = stats.Deaths + player.ServerSession.Deaths;

        float kd = kills / (deaths == 0 ? 1 : deaths);
        throw Context.Reply(PlayerStats, name, fish, kills, deaths, kd);
    }
}

[CommandParent(typeof(StatsCommand))]
[CommandData("session", "s")]
[CommandSyntax("[<Params: player?>]")]
internal class StatsSessionCommand : Command
{
    public StatsSessionCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation PlayerSessionStats = new("PlayerSessionStats");

    public override UniTask ExecuteAsync()
    {
        MeowPlayer target;
        if (Context.HasArguments(1))
        {
            target = Context.Parse<MeowPlayer>();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        Session stats = target.ServerSession;

        float kd = stats.Kills / (stats.Deaths == 0 ? 1 : stats.Deaths);
        throw Context.Reply(PlayerSessionStats, target.Name, stats.Fish, stats.Kills, stats.Deaths, kd);
    }
}

[CommandParent(typeof(StatsCommand))]
[CommandData("life", "l")]
[CommandSyntax("[<Params: player?>]")]
internal class StatsLifeCommand : Command
{
    public StatsLifeCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation PlayerLifeStats = new("PlayerStats");

    public override UniTask ExecuteAsync()
    {
        MeowPlayer target;
        if (Context.HasArguments(1))
        {
            target = Context.Parse<MeowPlayer>();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        Session stats = target.LifeSession;

        throw Context.Reply(PlayerLifeStats, target.Name, stats.Fish, stats.Kills);
    }
}
