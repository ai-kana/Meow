using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("reputation", "rep")]
[CommandSyntax("<[get,g | set,s | reset,r | add,a | take,t]>")]
internal class ReputationCommand : Command
{
    public ReputationCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        throw Context.Reply("<[get,g | set,s | reset,r | add,a | take,t]>");
    }
}

[CommandParent(typeof(ReputationCommand))]
[CommandData("get", "g")]
internal class ReputationGetCommand : Command
{
    public ReputationGetCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        KronstadtPlayer target = Context.Parse<KronstadtPlayer>();

        throw Context.Reply(TranslationList.CheckedReputation, target.Name, target.Quests.Reputation);
    }
}

[CommandParent(typeof(ReputationCommand))]
[CommandData("set", "s")]
[CommandSyntax("<player> <reputation>")]
internal class ReputationSetCommand : Command
{
    public ReputationSetCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        KronstadtPlayer target = Context.Parse<KronstadtPlayer>();
        Context.MoveNext();
        int reputation = Context.Parse<int>();

        target.Quests.SetReputation(reputation);
        
        throw Context.Reply(TranslationList.SetReputation, target.Name, reputation);
    }
}

[CommandParent(typeof(ReputationCommand))]
[CommandData("reset", "r")]
[CommandSyntax("<player>")]
internal class ReputationResetCommand : Command
{
    public ReputationResetCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        KronstadtPlayer target = Context.Parse<KronstadtPlayer>();

        target.Quests.SetReputation(0);
        
        throw Context.Reply(TranslationList.ResetReputation, target.Name);
    }
}

[CommandParent(typeof(ReputationCommand))]
[CommandData("add", "a")]
[CommandSyntax("<player> <reputation>")]
internal class ReputationAddCommand : Command
{
    public ReputationAddCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        KronstadtPlayer target = Context.Parse<KronstadtPlayer>();
        Context.MoveNext();
        int reputation = Context.Parse<int>();

        target.Quests.GiveReputation(reputation);
        
        throw Context.Reply(TranslationList.AddedReputation, reputation, target.Name);
    }
}

[CommandParent(typeof(ReputationCommand))]
[CommandData("take", "t")]
[CommandSyntax("<player> <reputation>")]
internal class ReputationTakeCommand : Command
{
    public ReputationTakeCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        KronstadtPlayer target = Context.Parse<KronstadtPlayer>();
        Context.MoveNext();
        int reputation = Context.Parse<int>();

        target.Quests.RemoveReputation(reputation);
        
        throw Context.Reply(TranslationList.TookReputation, reputation, target.Name);
    }
}
