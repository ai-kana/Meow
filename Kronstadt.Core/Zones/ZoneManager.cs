using Cysharp.Threading.Tasks;
using Kronstadt.Core.Logging;
using Kronstadt.Core.Players;
using Newtonsoft.Json;
using UnityEngine;

namespace Kronstadt.Core.Zones;

public delegate void ZoneEntered(KronstadtPlayer player, Zone zone);
public delegate void ZoneExited(KronstadtPlayer player, Zone zone);

public class ZoneManager
{
    private static readonly ILogger _Logger;
    static ZoneManager()
    {
        _Logger = LoggerProvider.CreateLogger<ZoneManager>();
    }

    private static List<Zone> _Zones = new();

    private const string ZonesFile = "Zones.json";
    public static async UniTask LoadZonesAsync()
    {
        if (!File.Exists(ZonesFile))
        {
            return;
        }

        using StreamReader reader = new(File.Open(ZonesFile, FileMode.Open, FileAccess.Read));
        string content = await reader.ReadToEndAsync();
        List<Zone> zones = JsonConvert.DeserializeObject<List<Zone>>(content) ?? throw new("Failed to load zones");

        _Zones.Capacity = zones.Count;

        await UniTask.Yield();
        foreach (Zone zone in zones)
        {
            AddZone(zone);
        }

        ServerManager.OnServerSave += OnServerSave;
    }

    private static void OnServerSave()
    {
        string content = JsonConvert.SerializeObject(_Zones);

        using StreamWriter writer = new(File.Open(ZonesFile, FileMode.Create, FileAccess.Write));
        writer.Write(content);
    }

    public static void AddZone(Zone zone)
    {
        _Zones.Add(zone);
    }

    public static bool HasFlag(KronstadtPlayer player, string flag)
    {
        foreach (Zone zone in _Zones)
        {
            float distance = Vector3.Distance(zone.Center.ToVector3(), player.Movement.Position);
            if (distance > zone.Radius)
            {
                continue;
            }

            if (zone.Flags.Contains(flag))
            {
                return true;
            }
        }

        return false;
    }
}
