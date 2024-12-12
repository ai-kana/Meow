using Meow.Core.Players;

namespace Meow.Core.Zones;

public class Zone
{
    public Position Center;
    public float Radius;
    public string[] Flags = [];

    public override int GetHashCode()
    {
        return Center.GetHashCode();
    }
}
