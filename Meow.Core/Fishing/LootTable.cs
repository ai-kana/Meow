namespace Meow.Core.Fishing;

public class LootTable
{
    private HashSet<LootItem> _Items = new();

    public LootTable() 
    {

    }

    public LootTable(IEnumerable<LootItem> items, uint playerLevel)
    {
        foreach (LootItem item in items.Where(x => playerLevel >= x.Level))
        {
            _Items.Add(item);
        }
    }

    public void Add(LootItem item)
    {
        _Items.Add(item);
    }

    public void Add(ushort id, ushort weight, ushort reward, uint level)
    {
        Add(new (id, weight, reward, level));
    }

    public LootItem GetItem()
    {
        int randomSum = 0;
        foreach (LootItem item in _Items)
        {
            randomSum += item.Weight;
        }

        if (randomSum == 0)
        {
            throw new Exception("Table is empty");
        }

        int random = new Random().Next(0, randomSum);

        int itemSum = 0;
        foreach (LootItem item in _Items)
        {
            itemSum += item.Weight;

            if (random <= itemSum)
            {
                return item;
            }
        }

        throw new Exception("Failed to find item");
    }
}
