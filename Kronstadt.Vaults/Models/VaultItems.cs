using SDG.Unturned;

namespace Kronstadt.Vaults.Models;

internal class VaultItems
{
    public Items Items = new(7);
    public string Name;

    public VaultItems(Vault vault, VaultData data)
    {
        Name = vault.Name;
        Items.resize(data.Width, data.Height);
        foreach (VaultItem item in vault.Items)
        {
            Items.addItem(item.X, item.Y, item.Rotation, new(item.Id, item.Amount, item.Quality, item.State));
        }
    }
}
