using Cysharp.Threading.Tasks;
using SDG.Unturned;
using Kronstadt.Core.Commands.Framework;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using Command = Kronstadt.Core.Commands.Framework.Command;

namespace Kronstadt.Core.Commands.StaffCommands;

[CommandData("clear")]
[CommandSyntax("[<Switches: ground, inventory>]")]
internal class ClearCommand : Command
{
    public ClearCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("clear");
        Context.AssertOnDuty();
        
        throw Context.Reply("[<Switches: ground, inventory>]");
    }
}

[CommandParent(typeof(ClearCommand))]
[CommandData("ground", "g")]
[CommandSyntax("[<Params: radius> <Switches: all>")]
internal class ClearGroundCommand : Command
{
    public ClearGroundCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer self);
        Context.AssertPermission("clear");
        Context.AssertOnDuty();
        
        float radius = Context.Parse<float>();

        ItemManager.ServerClearItemsInSphere(self.Movement.Position, radius);
        throw Context.Reply(TranslationList.ClearedGroundDistance, radius);
    }
}

[CommandParent(typeof(ClearGroundCommand))]
[CommandData("all", "a")]
internal class ClearGroundAllCommand : Command
{
    public ClearGroundAllCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out KronstadtPlayer self);
        Context.AssertPermission("clear");
        Context.AssertOnDuty();

        ItemManager.askClearAllItems();
        throw Context.Reply(TranslationList.ClearedGround);
    }
}

[CommandParent(typeof(ClearCommand))]
[CommandData("inventory", "i")]
[CommandSyntax("[?<Params: player>]")]
internal class ClearInventoryCommand : Command
{
    public ClearInventoryCommand(CommandContext context) : base(context)
    {
    }

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("clear");
        Context.AssertOnDuty();

        if (Context.HasExactArguments(1) && Context.TryParse(out KronstadtPlayer target))
        {
            Context.AssertPermission("clear.other");
            
            if(target.Inventory.ClearInventory() && target.Clothing.ClearClothes())
                throw Context.Reply(TranslationList.ClearedInventoryOther, target.Name);

            throw Context.Reply(TranslationList.FailedToClearInventoryOther, target.Name);
        }
        
        Context.AssertPlayer(out KronstadtPlayer self);
        
        if(self.Inventory.ClearInventory() && self.Clothing.ClearClothes())
            throw Context.Reply(TranslationList.ClearedInventorySelf);
        
        throw Context.Reply(TranslationList.FailedToClearInventorySelf);
    }
}
