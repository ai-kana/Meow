using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("reputation", "rep")]
[CommandSyntax("[<Switches: get, set, reset, add, take>]")]
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
[CommandSyntax("[<Params: player>]")]
internal class ReputationGetCommand : Command
{
    public ReputationGetCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        MeowPlayer target = Context.Parse<MeowPlayer>();

        throw Context.Reply(TranslationList.CheckedReputation, target.Name, target.Quests.Reputation);
    }
}

[CommandParent(typeof(ReputationCommand))]
[CommandData("set", "s")]
[CommandSyntax("[<Params: player>] [<Params: amount>]")]
internal class ReputationSetCommand : Command
{
    public ReputationSetCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        MeowPlayer target = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        int reputation = Context.Parse<int>();

        target.Quests.SetReputation(reputation);
        
        throw Context.Reply(TranslationList.SetReputation, target.Name, reputation);
    }
}

[CommandParent(typeof(ReputationCommand))]
[CommandData("reset", "r")]
[CommandSyntax("[<Params: player>]")]
internal class ReputationResetCommand : Command
{
    public ReputationResetCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        MeowPlayer target = Context.Parse<MeowPlayer>();

        target.Quests.SetReputation(0);
        
        throw Context.Reply(TranslationList.ResetReputation, target.Name);
    }
}

[CommandParent(typeof(ReputationCommand))]
[CommandData("add", "a")]
[CommandSyntax("[<Params: player>] [<Params: amount>]")]
internal class ReputationAddCommand : Command
{
    public ReputationAddCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        MeowPlayer target = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        int reputation = Context.Parse<int>();

        target.Quests.GiveReputation(reputation);
        
        throw Context.Reply(TranslationList.AddedReputation, reputation, target.Name);
    }
}

[CommandParent(typeof(ReputationCommand))]
[CommandData("take", "t")]
[CommandSyntax("[<Params: player>] [<Params: amount>]")]
internal class ReputationTakeCommand : Command
{
    public ReputationTakeCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("reputation");
        Context.AssertOnDuty();
        
        MeowPlayer target = Context.Parse<MeowPlayer>();
        Context.MoveNext();
        int reputation = Context.Parse<int>();

        target.Quests.RemoveReputation(reputation);
        
        throw Context.Reply(TranslationList.TookReputation, reputation, target.Name);
    }
}