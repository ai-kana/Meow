using Cysharp.Threading.Tasks;
using SDG.Unturned;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Extensions;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using Command = Kronstadt.Core.Commands.Framework.Command;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("giveitem", "gi")]
[CommandSyntax("[<Params: player>] [<Params: id, name>] [?<Params: amount>]")]
internal class GiveItemCommand : Command
{
    public GiveItemCommand(CommandContext context) : base(context)
    {
    }

    public bool GetItemAsset(string input, out ItemAsset? itemAsset)
    {
        input = input.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            itemAsset = null;
            return false;
        }

        List<ItemAsset> itemAssetsList = new();
        Assets.find(itemAssetsList);

        if (ushort.TryParse(input, out ushort id))
        {
            if (id == 0)
            {
                itemAsset = null;
                return false;
            }

            itemAsset = itemAssetsList.FirstOrDefault(i => i.id == id && !i.isPro);
            return itemAsset != null;
        }

        itemAsset = itemAssetsList.FirstOrDefault(i =>
            i.itemName.Contains(input, StringComparison.InvariantCultureIgnoreCase) ||
            i.name.Contains(input, StringComparison.InvariantCultureIgnoreCase) && !i.isPro);

        return itemAsset != null;
    }
    
    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("giveitem");
        Context.AssertOnDuty();
        Context.AssertArguments(2);
        
        KronstadtPlayer target = Context.Parse<KronstadtPlayer>();
        Context.MoveNext();

        if (!GetItemAsset(Context.Current, out ItemAsset? itemAsset))
        {
            throw Context.Reply(TranslationList.ItemNotFound);
        }
        
        if (Context.HasExactArguments(3))
        {
            Context.MoveNext();

            if (!Context.TryParse(out ushort count))
            {
                throw Context.Reply(TranslationList.BadNumber);
            }
                
            target.Inventory.GiveItems(itemAsset!.id, count);
            throw Context.Reply(TranslationList.GaveItemAmount, target.Name, count, itemAsset.FriendlyName, itemAsset.id);
        }
            
        target.Inventory.GiveItem(itemAsset!.id);
        throw Context.Reply(TranslationList.GaveItem, target.Name, itemAsset.FriendlyName, itemAsset.id);
    }
}
