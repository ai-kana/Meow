using Cysharp.Threading.Tasks;
using Meow.Core.Bot;
using Meow.Core.Commands.Framework;
using Meow.Core.Translations;

namespace Meow.Core.Commands;

[CommandData("link")]
internal class LinkCommand : Command
{
    public LinkCommand(CommandContext context) : base(context)
    {
    }

    private readonly static Translation LinkCodeMessage = new("LinkCodeMessage");

    public override async UniTask ExecuteAsync()
    {
        string code = await LinkManager.GetCodeAsync(Context.Caller.SteamID);
        if (code == null)
        {
            throw Context.Reply("Failed to find code");
        }

        throw Context.Reply(LinkCodeMessage, code);
    }
}
