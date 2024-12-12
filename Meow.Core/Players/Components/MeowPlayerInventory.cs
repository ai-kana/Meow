using SDG.Unturned;

namespace Meow.Core.Players.Components;

public class MeowPlayerInventory
{
    private PlayerInventory _Inventory => Owner.Player.inventory;
    private readonly MeowPlayer Owner;

    public MeowPlayerInventory(MeowPlayer owner)
    {
        Owner = owner;
    }
    
    public void GiveItem(ushort id)
    {
        _Inventory.forceAddItem(new Item(id, EItemOrigin.ADMIN), true);
    }
    
    public void GiveItem(ushort id, byte amount, byte quality)
    {
        _Inventory.forceAddItem(new Item(id, amount, quality), true);
    }
    
    public void GiveItem(ushort id, byte amount, byte quality, byte[] state)
    {
        _Inventory.forceAddItem(new Item(id, amount, quality, state), true);
    }
    
    public void GiveItems(ushort id, ushort count)
    {
        for (int i = 0; i < count; i++)
        {
            GiveItem(id);
        }
    }
    
    public void GiveItems(ushort id, byte amount, byte quality, ushort count)
    {
        for (int i = 0; i < count; i++)
        {
            GiveItem(id, amount, quality);
        }
    }
    
    public void GiveItems(ushort id, byte amount, byte quality, byte[] state, ushort count)
    {
        for (int i = 0; i < count; i++)
        {
            GiveItem(id, amount, quality, state);
        }
    }
    
    public bool ClearInventory()
    {
        for (int page = 0; page < PlayerInventory.PAGES - 2; page++)
        {
            int count = _Inventory.getItemCount((byte)page);
            if(count == 0) continue;

            for (int index = 0; index < count; index++)
            {
                _Inventory.removeItem((byte)page, 0);
            }
        }
        
        return true;
    }
    
    public void ClearHands() 
    {
        for (byte itemCount = 0; itemCount < _Inventory.getItemCount(2); itemCount++)
        {
            _Inventory.removeItem(2, 0);
        }
    }
}
