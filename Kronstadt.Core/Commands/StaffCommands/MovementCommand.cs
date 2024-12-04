using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("movement", "move")]
[CommandSyntax("<[speed,s | jump,j | gravity,g]> <[value] | reset, r>")]
internal class MovementCommand : Command
{
    public MovementCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("movement");
        Context.AssertOnDuty();
        
        throw Context.Reply("<[speed,s | jump,j | gravity,g]> <[value] | reset, r>");
    }
}

[CommandParent(typeof(MovementCommand))]
[CommandData("speed", "s")]
internal class MovementSpeedCommand : Command
{
    public MovementSpeedCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("movement");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        KronstadtPlayer target;
        if (Context.HasArguments(2))
        {
            target = Context.Parse<KronstadtPlayer>();
            Context.MoveNext();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        float multiplier = Context.MatchParameter("reset", "r") ? 1f : Context.Parse<float>();
        target.Movement.SetSpeed(multiplier);
        throw Context.HasArguments(2) ?
            Context.Reply(TranslationList.SetSpeedOther, target.Name, multiplier)
            : Context.Reply(TranslationList.SetSpeedSelf, multiplier);
    }
}

[CommandParent(typeof(MovementCommand))]
[CommandData("jump", "j")]
internal class MovementJumpCommand : Command
{
    public MovementJumpCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("movement");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        KronstadtPlayer target;
        if (Context.HasArguments(2))
        {
            target = Context.Parse<KronstadtPlayer>();
            Context.MoveNext();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        float multiplier = Context.MatchParameter("reset", "r") ? 1f : Context.Parse<float>();
        target.Movement.SetJump(multiplier);
        throw Context.HasArguments(2) ?
            Context.Reply(TranslationList.SetJumpOther, target.Name, multiplier)
            : Context.Reply(TranslationList.SetJumpSelf, multiplier);
    }
}

[CommandParent(typeof(MovementCommand))]
[CommandData("gravity", "g")]
internal class MovementGravityCommand : Command
{
    public MovementGravityCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("movement");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        KronstadtPlayer target;
        if (Context.HasArguments(2))
        {
            target = Context.Parse<KronstadtPlayer>();
            Context.MoveNext();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        float multiplier = Context.MatchParameter("reset", "r") ? 1f : Context.Parse<float>();
        target.Movement.SetGravity(multiplier);
        throw Context.HasArguments(2) ?
            Context.Reply(TranslationList.SetGravityOther, target.Name, multiplier)
            : Context.Reply(TranslationList.SetGravitySelf, multiplier);
    }
}
