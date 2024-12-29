using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Players.Components;
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
            
            if (amount > caller.Skills.Experience)
            {
                throw Context.Reply(LessThanX, caller.Skills.Experience+1);
            }

            caller.Skills.RemoveExperience(amount);
            caller.SendMessage(PlayerPaid, amount, target);

            target.Skills.GiveExperience(amount);
            target.SendMessage(PlayerReceived, amount, caller);

            throw Context.Exit;
        }
    }
}
