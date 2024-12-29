using Cysharp.Threading.Tasks;
using SDG.Unturned;
using Meow.Core.Commands.Framework;
using Meow.Core.Extensions;
using Meow.Core.Players;
using Meow.Core.Translations;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands.StaffCommands;

[CommandData("giveitem", "gi")]
[CommandSyntax("[<Params: player>] [<Params: id, name>] [?<Params: amount>]")]
internal class GiveItemCommand : Command
{
    public GiveItemCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation GaveItem = new("GaveItem");
    private static readonly Translation GaveItemAmount = new("GaveItemAmount");
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
        Context.AssertPermission("giveitem");
        Context.AssertOnDuty();
        Context.AssertArguments(2);
        
        MeowPlayer target = Context.Parse<MeowPlayer>();
        Context.MoveNext();

        if (!GetItemAsset(Context.Current, out ItemAsset? itemAsset))
        {
            throw Context.Reply(ItemNotFound);
        }
        
        if (Context.HasExactArguments(3))
        {
            Context.MoveNext();

            if (!Context.TryParse(out ushort count))
            {
                throw Context.Reply(TranslationList.BadNumber);
            }
                
            target.GiveItems(itemAsset!.id, count);
            throw Context.Reply(GaveItemAmount, target.Name, count, itemAsset.FriendlyName, itemAsset.id);
        }
            
        target.GiveItem(itemAsset!.id);
        throw Context.Reply(GaveItem, target.Name, itemAsset.FriendlyName, itemAsset.id);
    }
}
