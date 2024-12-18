using Cysharp.Threading.Tasks;
using SDG.Unturned;
using Meow.Core.Commands.Framework;
using Meow.Core.Players;
using Meow.Core.Translations;
using Command = Meow.Core.Commands.Framework.Command;

namespace Meow.Core.Commands.StaffCommands;

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

    private static readonly Translation ClearedGroundDistance = new("ClearedGroundDistance");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer self);
        Context.AssertPermission("clear");
        Context.AssertOnDuty();
        
        float radius = Context.Parse<float>();

        ItemManager.ServerClearItemsInSphere(self.Movement.Position, radius);
        throw Context.Reply(ClearedGroundDistance, radius);
    }
}

[CommandParent(typeof(ClearGroundCommand))]
[CommandData("all", "a")]
internal class ClearGroundAllCommand : Command
{
    public ClearGroundAllCommand(CommandContext context) : base(context)
    {
    }

    private static readonly Translation ClearedGround = new("ClearedGround");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPlayer(out MeowPlayer self);
        Context.AssertPermission("clear");
        Context.AssertOnDuty();

        ItemManager.askClearAllItems();
        throw Context.Reply(ClearedGround);
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

    private static readonly Translation ClearedInventoryOther = new("ClearedInventoryOther");
    private static readonly Translation ClearedInventorySelf = new("ClearedInventorySelf");
    private static readonly Translation FailedToClearInventorySelf = new("FailedToClearInventorySelf");
    private static readonly Translation FailedToClearInventoryOther = new("FailedToClearInventoryOther");

    public override UniTask ExecuteAsync()
    {
        Context.AssertPermission("clear");
        Context.AssertOnDuty();

        if (Context.HasExactArguments(1) && Context.TryParse(out MeowPlayer target))
        {
            Context.AssertPermission("clear.other");
            
            if(target.Inventory.ClearInventory() && target.Clothing.ClearClothes())
                throw Context.Reply(ClearedInventoryOther, target.Name);

            throw Context.Reply(FailedToClearInventoryOther, target.Name);
        }
        
        Context.AssertPlayer(out MeowPlayer self);
        
        if(self.Inventory.ClearInventory() && self.Clothing.ClearClothes())
            throw Context.Reply(ClearedInventorySelf);
        
        throw Context.Reply(FailedToClearInventorySelf);
    }
}
