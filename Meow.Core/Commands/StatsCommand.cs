using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Players.Components;
using Meow.Core.Stats;
using Meow.Core.Translations;
using Steamworks;

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
        MeowPlayer? player = null;
        if (!Context.TryParse<CSteamID>(out CSteamID target))
        {
            Context.AssertPlayer(out player);
            target = player.SteamID;
        }

        if (player == null)
        {
            Context.TryParse(out player);
        }

        string name = player?.Name ?? target.ToString();

        PlayerStats? stats = await StatsManager.GetStats(target);
        if (stats == null)
        {
            throw Context.Reply(FailedToGetStats, name);
        }

        uint fish = stats.Fish + player?.Stats.ServerSession.Fish ?? 0;
        uint kills = stats.Kills + player?.Stats.ServerSession.Kills ?? 0;
        uint deaths = stats.Deaths + player?.Stats.ServerSession.Deaths ?? 0;

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
        MeowPlayer? target;
        if (Context.HasArguments(1))
        {
            target = Context.Parse<MeowPlayer>();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        MeowPlayerStats.Session stats = target.Stats.ServerSession;

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
        MeowPlayer? target;
        if (Context.HasArguments(1))
        {
            target = Context.Parse<MeowPlayer>();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        MeowPlayerStats.Session stats = target.Stats.LifeSession;

        throw Context.Reply(PlayerLifeStats, target.Name, stats.Fish, stats.Kills);
    }
}
