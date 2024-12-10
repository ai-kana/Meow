using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands.StaffCommands;

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
            KronstadtPlayer player = Context.Parse<KronstadtPlayer>();
            state = player.Administration.ToggleGod();
            
            throw Context.Reply(TranslationList.GodModeOther, player.Name, state ? new TranslationPackage(TranslationList.On) : new TranslationPackage(TranslationList.Off));
        }

        Context.AssertPlayer(out KronstadtPlayer self);
        state = self.Administration.ToggleGod();

        throw Context.Reply(TranslationList.GodModeSelf, state ? new TranslationPackage(TranslationList.On) : new TranslationPackage(TranslationList.Off));
    }
}
