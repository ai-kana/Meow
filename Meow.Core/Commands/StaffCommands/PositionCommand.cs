using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("position", "pos")]
[CommandSyntax("[<Params: player?>]")]
internal class PositionCommand : Command
{
    public PositionCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation PositionSelf = new("PositionSelf");
    private static readonly Translation PositionTarget = new("PositionTarget");
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertOnDuty();
        Context.AssertPermission("position");

        string x,y,z;
        
        if (Context.HasExactArguments(0))
        {
            Context.AssertPlayer(out MeowPlayer self);
            
            x = self.Position.x.ToString("F1");
            y = self.Position.y.ToString("F1");
            z = self.Position.z.ToString("F1");
            
            throw Context.Reply(PositionSelf, x, y, z);
        }
        
        Context.AssertArguments(1);
        MeowPlayer target = Context.Parse<MeowPlayer>();
        
        x = target.Position.x.ToString("F1");
        y = target.Position.y.ToString("F1");
        z = target.Position.z.ToString("F1");
        
        throw Context.Reply(PositionTarget, target.Name, x, y, z);
    }
}
