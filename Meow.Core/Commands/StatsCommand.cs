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

    public static readonly Translation FailedToGetStats = new("FailedToGetStats", "Failed to get {0}'s stats");
    private static readonly Translation PlayerStats = new("PlayerStats", "{0}'s stats: {1} fish caught {2} kills {3} deaths K/D {4:F2}");

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

        float kd = stats.Kills / (stats.Deaths == 0 ? 1 : stats.Deaths);
        throw Context.Reply(PlayerStats, name, stats.Fish, stats.Kills, stats.Deaths, kd);
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

    private static readonly Translation PlayerSessionStats = new("PlayerSessionStats", "{0}'s session stats: {1} fish caught {2} kills {3} deaths K/D {4:F2}");

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

    private static readonly Translation PlayerLifeStats = new("PlayerStats", "{0}'s life stats: {1} fish caught {2} kills");

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
