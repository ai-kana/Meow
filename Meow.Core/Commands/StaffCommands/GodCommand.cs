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
            
            throw Context.Reply(TranslationList.GodModeOther, player.Name, state ? new TranslationPackage(TranslationList.On) : new TranslationPackage(TranslationList.Off));
        }

        Context.AssertPlayer(out MeowPlayer self);
        state = self.Administration.ToggleGod();

        throw Context.Reply(TranslationList.GodModeSelf, state ? new TranslationPackage(TranslationList.On) : new TranslationPackage(TranslationList.Off));
    }
}
