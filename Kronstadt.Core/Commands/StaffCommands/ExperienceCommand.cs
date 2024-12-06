using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("experience", "exp", "xp")]
[CommandSyntax("<[add,a | remove,r | set,s | reset | check,c]>")]
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
        throw Context.Reply("<add | remove | set | check>");
    }
}

[CommandParent(typeof(ExperienceCommand))]
[CommandData("add", "a")]
[CommandSyntax("<[player]> <[Amount]>")]
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

        KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
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
[CommandSyntax("<[player]> <[Amount]>")]
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

        KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
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

        KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
        
        player.Skills.SetExperience(0);
        
        throw Context.Reply(TranslationList.ResetExperience, player.Name);
    }
}

[CommandParent(typeof(ExperienceCommand))]
[CommandData("set", "s")]
[CommandSyntax("<[player]> <[Amount]>")]
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

        KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
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
[CommandSyntax("<[player]>")]
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

        KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
        
        throw Context.Reply(TranslationList.CheckedExperience, player.Name, player.Skills.Experience);
    }
}
