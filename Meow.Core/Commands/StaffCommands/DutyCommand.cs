using Cysharp.Threading.Tasks;
using Meow.Core.Chat;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("duty", "d")]
[CommandSyntax("[<Switches: silent, check>]")]
internal class DutyCommand : Command
{
    public DutyCommand(CommandContext context) : base(context)
    {
    }
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("duty");
        Context.AssertPlayer(out MeowPlayer caller);

        bool state = caller.Administration.ToggleDuty();
        MeowChat.BroadcastMessage(TranslationList.DutyStateGlobal, caller.Name, state ? new TranslationPackage(TranslationList.On) : new TranslationPackage(TranslationList.Off));
        throw Context.Exit;
    }
}

[CommandParent(typeof(DutyCommand))]
[CommandData("silent", "s")]
internal class DutySlientCommand : Command
{
    public DutySlientCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("sduty");
        Context.AssertPlayer(out MeowPlayer caller);

        bool state = caller.Administration.ToggleDuty();
        throw Context.Reply(TranslationList.DutyStateSilent, state ? new TranslationPackage(TranslationList.On) : new TranslationPackage(TranslationList.Off));
    }
}

[CommandParent(typeof(DutyCommand))]
[CommandData("check", "c")]
internal class DutyCheckCommand : Command
{
    public DutyCheckCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("duty");
        Context.AssertPlayer(out MeowPlayer caller);

        throw Context.Reply(TranslationList.DutyStateCheck, caller.Administration.OnDuty ? new TranslationPackage(TranslationList.On) : new TranslationPackage(TranslationList.Off));
    }
}
