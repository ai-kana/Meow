using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("experience", "exp", "xp")]
[CommandSyntax("[<Switches: add, remove, set, reset, check>]")]
internal class ExperienceCommand : Command
{
    public ExperienceCommand (CommandContext context) : base(context)
    {
    }

    public static bool IsXpValid(uint xp)
    {
        return xp is > 0 and < uint.MaxValue;
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("experience");
        Context.AssertOnDuty();
        throw Context.Reply("[<Switches: add, remove, set, reset, check>]");
    }
}

[CommandParent(typeof(ExperienceCommand))]
[CommandData("add", "a")]
[CommandSyntax("[<Params: player>] [<Params: amount>]")]
internal class ExperienceAddCommand : Command 
{
    public ExperienceAddCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("experience");
        Context.AssertOnDuty();
        Context.AssertArguments(2);

        MeowPlayer player = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        uint amount = Context.Parse<uint>();

        if (!ExperienceCommand.IsXpValid(amount))
        {
            throw Context.Reply(TranslationList.BadNumber);
        }
        
        player.Skills.GiveExperience(amount);
        
        throw Context.Reply(TranslationList.AddedExperience, amount, player.Name);
    }
}

[CommandParent(typeof(ExperienceCommand))]
[CommandData("remove", "r")]
[CommandSyntax("[<Params: player>] [<Params: amount>]")]
internal class ExperienceRemoveCommand : Command
{
    public ExperienceRemoveCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("experience");
        Context.AssertOnDuty();
        Context.AssertArguments(2);

        MeowPlayer player = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        uint amount = Context.Parse<uint>();

        if (!ExperienceCommand.IsXpValid(amount))
        {
            throw Context.Reply(TranslationList.BadNumber);
        }
        
        player.Skills.RemoveExperience(amount);
        
        throw Context.Reply(TranslationList.RemovedExperience, amount, player.Name);
    }
}

[CommandParent(typeof(ExperienceCommand))]
[CommandData("reset")]
[CommandSyntax("[<Params: player>]")]
internal class ExperienceResetCommand : Command
{
    public ExperienceResetCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("experience");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        MeowPlayer player = Context.Parse<MeowPlayer>();
        
        player.Skills.SetExperience(0);
        
        throw Context.Reply(TranslationList.ResetExperience, player.Name);
    }
}

[CommandParent(typeof(ExperienceCommand))]
[CommandData("set", "s")]
[CommandSyntax("[<Params: player>] [<Params: amount>]")]
internal class ExperienceSetCommand : Command
{
    public ExperienceSetCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("experience");
        Context.AssertOnDuty();
        Context.AssertArguments(2);

        MeowPlayer player = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        uint amount = Context.Parse<uint>();

        if (!ExperienceCommand.IsXpValid(amount))
        {
            throw Context.Reply(TranslationList.BadNumber);
        }
        
        player.Skills.SetExperience(amount);
        
        throw Context.Reply(TranslationList.SetExperience, player.Name, amount);
    }
}

[CommandParent(typeof(ExperienceCommand))]
[CommandData("check", "c")]
[CommandSyntax("[<Params: player>]")]
internal class ExperienceCheckCommand : Command
{
    public ExperienceCheckCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("experience");
        Context.AssertOnDuty();
        Context.AssertArguments(1);

        MeowPlayer player = Context.Parse<MeowPlayer>();
        
        throw Context.Reply(TranslationList.CheckedExperience, player.Name, player.Skills.Experience);
    }
}
