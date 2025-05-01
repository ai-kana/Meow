using Cysharp.Threading.Tasks;
using Meow.Core.Chat;
using Meow.Core.Commands.Framework;
using Meow.Core.Translations;

namespace Meow.Core.Commands;

[CommandData("save")]
internal class SaveCommand : Command
{
    public SaveCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation ServerSaving = new("ServerSave");
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("save");
        Context.AssertOnDuty();

        ServerManager.StartSave();
        MeowChat.BroadcastMessage(ServerSaving);

        throw Context.Exit;
    }
}
