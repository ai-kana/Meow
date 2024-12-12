using Newtonsoft.Json;
using SDG.Unturned;

namespace Meow.Vaults.Models;

[Serializable]
internal class Vault
{
    public Vault(string name)
    {
        Name = name;
    }

    public Vault(VaultItems items)
    {
        Name = items.Name;
        List<VaultItem> final = new(items.Items.getItemCount());
        foreach (ItemJar jar in items.Items.items)
        {
            final.Add(new(jar));
        }

        Items = final;
    }

    public Vault()
    {
    }

    [JsonProperty]
    public string Name {get; private set;} = "default";
    [JsonProperty]
    public IEnumerable<VaultItem> Items {get; private set;} = [];
}
