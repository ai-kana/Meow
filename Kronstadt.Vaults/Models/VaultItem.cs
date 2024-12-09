using Newtonsoft.Json;
using SDG.Unturned;

namespace Kronstadt.Vaults.Models;

[Serializable]
internal class VaultItem
{
    public VaultItem(ItemJar jar)
    {
        Item item = jar.item;
        Id = item.id;
        Amount = item.amount;
        Quality = item.quality;
        State = item.state;

        X = jar.x;
        Y = jar.y;
        Rotation = jar.rot;
    }

    public VaultItem()
    {
        State = [];
    }

    [JsonProperty]
    public ushort Id {get; private set;}
    [JsonProperty]
    public byte Amount {get; private set;}
    [JsonProperty]
    public byte Quality {get; private set;}
    [JsonProperty]
    public byte[] State {get; private set;}

    [JsonProperty]
    public byte X {get; private set;}
    [JsonProperty]
    public byte Y {get; private set;}
    [JsonProperty]
    public byte Rotation {get; private set;}
}
