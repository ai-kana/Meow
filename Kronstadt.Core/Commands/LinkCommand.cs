using Cysharp.Threading.Tasks;
using Kronstadt.Core.Bot;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Translations;

namespace Kronstadt.Core.Commands;

[CommandData("link")]
internal class LinkCommand : Command
{
    public LinkCommand(CommandContext context) : base(context)
    {
    }

    private readonly static Translation LinkCodeMessage = new("LinkCodeMessage", "Do /link {0} in our discord");

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
