namespace Meow.Core.Players.Components;

public class MeowPlayerCooldowns
{
    public Dictionary<string, long> Cooldowns => Owner.SaveData.Cooldowns;

    public readonly MeowPlayer Owner;
    public MeowPlayerCooldowns(MeowPlayer owner)
    {
        Owner = owner;
    }

    public long GetCooldown(string id)
    {
        long now = DateTimeOffset.Now.ToUnixTimeSeconds();
        if (!Cooldowns.ContainsKey(id))
        {
            return 0;
        }

        long remaining = Cooldowns[id] - now;
        if (remaining < 0)
        {
            Cooldowns.Remove(id);
            return 0;
        }

        return remaining;
    }

    public void AddCooldown(string id, long length)
    {
        long end = DateTimeOffset.Now.ToUnixTimeSeconds() + length;
        if (Cooldowns.ContainsKey(id))
        {
            Cooldowns[id] = end;
            return;
        }

        Cooldowns.Add(id, end);
    }

}
