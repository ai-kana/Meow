using Cysharp.Threading.Tasks;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Formatting;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using Kronstadt.Vaults.Models;

namespace Kronstadt.Vaults.Commands;

[CommandData("vault")]
internal class VaultCommand : Command
{
    public VaultCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation VaultNotFound = new("VaultNotFound", "Cannot find vault named {0}");

    public override async UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer caller);

        string name = "default";
        if (Context.HasArguments(1))
        {
            name = Context.Current;
        }

        if (!VaultPlugin.Vaults.TryGetValue(caller.SteamID, out IEnumerable<VaultItems> items))
        {
            throw Context.Reply("Failed to get vault set; should never happen");
        }

        VaultItems? vaultItems = items.FirstOrDefault(x => string.Compare(x.Name, name, true) == 0);
        if (vaultItems == null)
        {
            throw Context.Reply(VaultNotFound, name);
        }

        await UniTask.Yield();

        caller.Player.inventory.storage = null;
        caller.Player.inventory.isStoring = true;
        caller.Player.inventory.updateItems(7, vaultItems.Items);
        caller.Player.inventory.sendStorage();

        throw Context.Exit;
    }
}

[CommandParent(typeof(VaultCommand))]
[CommandData("list")]
internal class VaultListCommand : Command
{
    public VaultListCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation VaultList = new("VaultList", "You have access to vaults {0}");
    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer caller);

        string name = "default";
        if (Context.HasArguments(1))
        {
            name = Context.Current;
        }

        if (!VaultPlugin.Vaults.TryGetValue(caller.SteamID, out IEnumerable<VaultItems> items))
        {
            throw Context.Reply("Failed to get vault set; should never happen");
        }

        string list = Formatter.FormatList(items.Select(x => x.Name), ", ");
        throw Context.Reply(VaultList, list);
    }
}
