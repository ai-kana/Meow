using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Translations;

namespace Meow.Core.Commands;

[CommandData("pardon")]
[CommandSyntax("[<Params: banid>]")]
internal class PardonCommand : Command
{
    public PardonCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation PardonFailed = new("PardonFailed");
    private static readonly Translation Pardoned = new("Pardoned");

    public override async UniTask ExecuteAsync()
    {
        Context.AssertOnDuty();
        Context.AssertPermission("pardon");
        Context.AssertArguments(1);

        int id = Context.Parse<int>();
        if (!await Offenses.OffenseManager.PardonOffense(id))
        {
            throw Context.Reply(PardonFailed);
        }

        throw Context.Reply(Pardoned, id);
    }
}
