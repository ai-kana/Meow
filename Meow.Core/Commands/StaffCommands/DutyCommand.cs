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
    
    private static readonly Translation DutyStateGlobal = new("DutyStateGlobal");
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("duty");
        Context.AssertPlayer(out MeowPlayer caller);

        bool state = caller.Administration.ToggleDuty();
        MeowChat.BroadcastMessage(DutyStateGlobal, caller.Name, state ? TranslationList.On.AsPackage() : TranslationList.Off.AsPackage());
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

    private static readonly Translation DutyStateSilent = new("DutyStateSilent");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("sduty");
        Context.AssertPlayer(out MeowPlayer caller);

        bool state = caller.Administration.ToggleDuty();
        throw Context.Reply(DutyStateSilent, state ? TranslationList.On.AsPackage() : TranslationList.Off.AsPackage());
    }
}

[CommandParent(typeof(DutyCommand))]
[CommandData("check", "c")]
internal class DutyCheckCommand : Command
{
    public DutyCheckCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation DutyStateCheck = new("DutyStateCheck");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("duty");
        Context.AssertPlayer(out MeowPlayer caller);

        throw Context.Reply(DutyStateCheck, caller.Administration.OnDuty ? TranslationList.On.AsPackage() : TranslationList.Off.AsPackage());
    }
}
