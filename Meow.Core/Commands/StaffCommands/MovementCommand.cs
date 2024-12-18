using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("movement", "move")]
[CommandSyntax("[<Switches: speed, jump, gravity>]")]
internal class MovementCommand : Command
{
    public MovementCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("movement");
        Context.AssertOnDuty();
        
        throw Context.Reply("[<Switches: speed, jump, gravity>]");
    }
}

[CommandParent(typeof(MovementCommand))]
[CommandData("speed", "s")]
[CommandSyntax("[<Params: multiplier, reset>]")]
internal class MovementSpeedCommand : Command
{
    public MovementSpeedCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation SetSpeedOther = new("SetSpeedOther");
    private static readonly Translation SetSpeedSelf = new("SetSpeedSelf");
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("movement");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        MeowPlayer target;
        if (Context.HasArguments(2))
        {
            target = Context.Parse<MeowPlayer>();
            Context.MoveNext();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        float multiplier = Context.MatchParameter("reset", "r") ? 1f : Context.Parse<float>();
        target.Movement.SetSpeed(multiplier);
        throw Context.HasArguments(2) ?
            Context.Reply(SetSpeedOther, target.Name, multiplier)
            : Context.Reply(SetSpeedSelf, multiplier);
    }
}

[CommandParent(typeof(MovementCommand))]
[CommandData("jump", "j")]
[CommandSyntax("[<Params: multiplier, reset>]")]
internal class MovementJumpCommand : Command
{
    public MovementJumpCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation SetJumpOther = new("SetJumpOther");
    private static readonly Translation SetJumpSelf = new("SetJumpSelf");
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("movement");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        MeowPlayer target;
        if (Context.HasArguments(2))
        {
            target = Context.Parse<MeowPlayer>();
            Context.MoveNext();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        float multiplier = Context.MatchParameter("reset", "r") ? 1f : Context.Parse<float>();
        target.Movement.SetJump(multiplier);
        throw Context.HasArguments(2) ?
            Context.Reply(SetJumpOther, target.Name, multiplier)
            : Context.Reply(SetJumpSelf, multiplier);
    }
}

[CommandParent(typeof(MovementCommand))]
[CommandData("gravity", "g")]
[CommandSyntax("[<Params: multiplier, reset>]")]
internal class MovementGravityCommand : Command
{
    public MovementGravityCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation SetGravityOther = new("SetGravityOther");
    private static readonly Translation SetGravitySelf = new("SetGravitySelf");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("movement");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        MeowPlayer target;
        if (Context.HasArguments(2))
        {
            target = Context.Parse<MeowPlayer>();
            Context.MoveNext();
        }
        else
        {
            Context.AssertPlayer(out target);
        }

        float multiplier = Context.MatchParameter("reset", "r") ? 1f : Context.Parse<float>();
        target.Movement.SetGravity(multiplier);
        throw Context.HasArguments(2) ?
            Context.Reply(SetGravityOther, target.Name, multiplier)
            : Context.Reply(SetGravitySelf, multiplier);
    }
}
