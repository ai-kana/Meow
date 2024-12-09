using Newtonsoft.Json;
using SDG.Unturned;

namespace Kronstadt.Vaults.Models;

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
        foreach (ItemJar jar in items.Items.items)
        {
            Items.Add(new(jar));
        }
    }

    public Vault()
    {
    }

    [JsonProperty]
    public string Name {get; private set;} = "default";
    [JsonProperty]
    public List<VaultItem> Items {get; private set;} = new();
}
