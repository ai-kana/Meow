using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("god")]
[CommandSyntax("[?<Params: player>]")]
internal class GodCommand : Command
{
    public GodCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation GodModeSelf = new("GodModeSelf");
    private static readonly Translation GodModeOther = new("GodModeOther");
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("god");
        Context.AssertOnDuty();

        bool state;
        if (Context.HasArguments(1))
        {
            Context.AssertPermission("god.other");
            MeowPlayer player = Context.Parse<MeowPlayer>();
            state = player.Administration.ToggleGod();
            
            throw Context.Reply(GodModeOther, player.Name, state ? TranslationList.On.AsPackage() : TranslationList.Off.AsPackage());
        }

        Context.AssertPlayer(out MeowPlayer self);
        state = self.Administration.ToggleGod();

        throw Context.Reply(GodModeSelf, state ? TranslationList.On.AsPackage() : TranslationList.Off.AsPackage());
    }
}
