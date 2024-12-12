using SDG.Unturned;

namespace Meow.Core.Players.Components;

public class MeowPlayerClothing
{
    private PlayerClothing _Clothing => Owner.Player.clothing;
    private readonly MeowPlayer Owner;

    public MeowPlayerClothing(MeowPlayer owner)
    {
        Owner = owner;
    }

    public bool ClearClothes()
    {
        byte[] state = [];
        
        _Clothing.askWearHat(0, 0, state, false);
        Owner.Inventory.ClearHands();
        
        _Clothing.askWearGlasses(0, 0, state, false);
        Owner.Inventory.ClearHands();
        
        _Clothing.askWearMask(0, 0, state, false);
        Owner.Inventory.ClearHands();
        
        _Clothing.askWearShirt(0, 0, state, false);
        Owner.Inventory.ClearHands();
        
        _Clothing.askWearBackpack(0, 0, state, false);
        Owner.Inventory.ClearHands();
        
        _Clothing.askWearVest(0, 0, state, false);
        Owner.Inventory.ClearHands();
        
        _Clothing.askWearPants(0, 0, state, false);
        Owner.Inventory.ClearHands();
        
        return true;
    }
}
