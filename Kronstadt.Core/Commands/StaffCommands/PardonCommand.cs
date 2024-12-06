using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands;

[CommandData("pardon")]
internal class PardonCommand : Command
{
    public PardonCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation PardonFailed = new("PardonFailed", "Failed to pardon offense");
    private static readonly Translation Pardoned = new("Pardoned", "Pardoned offense #{0}");

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
