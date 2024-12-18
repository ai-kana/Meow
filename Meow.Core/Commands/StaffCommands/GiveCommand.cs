using Cysharp.Threading.Tasks;
using SDG.Unturned;
using Meow.Core.Commands.Framework;
using Meow.Core.Extensions;
using Meow.Core.Players;
using Meow.Core.Translations;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("give", "item", "i")]
[CommandSyntax("[<Params: id, name>] [?<Params: amount>]")]
internal class GiveCommand : Command
{
    public GiveCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation ItemSelfAmount = new("ItemSelfAmount");
    private static readonly Translation ItemSelf = new("ItemSelf");
    private static readonly Translation ItemNotFound = new("ItemNotFound");

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
        Context.AssertPermission("give");
        Context.AssertOnDuty();
        Context.AssertArguments(1);
        Context.AssertPlayer(out MeowPlayer self);

        if (!GetItemAsset(Context.Current, out ItemAsset? itemAsset))
        {
            throw Context.Reply(ItemNotFound);
        }
        
        if (Context.HasExactArguments(2))
        {
            Context.MoveNext();

            if (!Context.TryParse(out ushort count))
            {
                throw Context.Reply(TranslationList.BadNumber);
            }
                
            self.Inventory.GiveItems(itemAsset!.id, count);
            throw Context.Reply(ItemSelfAmount, count, itemAsset.FriendlyName, itemAsset.id);
        }
            
        self.Inventory.GiveItem(itemAsset!.id);
        throw Context.Reply(ItemSelf, itemAsset.FriendlyName, itemAsset.id);
    }
}
