using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Unturnov.Commands
{
    [CommandData("pay")]
    [CommandSyntax("[<Params: player>] [<Params: amount>]")]
    public class PayCommand : Command
    {
        public PayCommand(CommandContext context) : base(context)
        {
        }

        private static readonly Translation LessThanX = new("LessThanX");
        private static readonly Translation PlayerPaid = new("PlayerPaid");
        private static readonly Translation PlayerReceived = new("PlayerReceived");

        public override UniTask ExecuteAsync()
        {
            Context.AssertArguments(2);

            Context.AssertPlayer(out MeowPlayer caller);
            MeowPlayer target = Context.Parse<MeowPlayer>();

            Context.MoveNext();
            uint amount = Context.Parse<uint>();
            
            if (amount > caller.Experience)
            {
                throw Context.Reply(LessThanX, caller.Experience+1);
            }

            caller.RemoveExperience(amount);
            caller.SendMessage(PlayerPaid, amount, target);

            target.GiveExperience(amount);
            target.SendMessage(PlayerReceived, amount, caller);

            throw Context.Exit;
        }
    }
}
