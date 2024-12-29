using Newtonsoft.Json;
using UnityEngine;
using Meow.Core.Fishing;

namespace Meow.Core.Players;

public struct Position
{ 
    public float X = 0;
    public float Y = 0;
    public float Z = 0;

    public readonly bool IsZero() => X == 0 && Y == 0 && Z == 0;

    public static implicit operator Position(in Vector3 vector) => new() 
    {
        X = vector.x,
        Y = vector.y,
        Z = vector.z
    };

    public readonly Vector3 ToVector3() => new(X, Y, Z);
    public static Position FromVector3(in Vector3 v) => new() {X = v.x, Y = v.y, Z = v.z};

    public Position()
    {
    }
}

[Serializable]
public class PlayerData
{
    [JsonProperty]
    public HashSet<string> Permissions {get; private set;} = new();
    [JsonProperty]
    public HashSet<string> Roles {get; private set;} = new();
    // String - Cooldown Name
    // long - Unix time stamp for end
    [JsonProperty]
    public Dictionary<string, long> Cooldowns {get; private set;} = new();

    [JsonProperty]
    public string Language {get; set;} = "English";

    [JsonProperty]
    public FishingSkill Fishing {get; private set;} = new();

    [JsonProperty]
    public Dictionary<byte, string> CustomBinds {get; private set;} = new();
}
