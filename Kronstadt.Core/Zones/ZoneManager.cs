using Cysharp.Threading.Tasks;
using Kronstadt.Core.Logging;
using Kronstadt.Core.Players;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Kronstadt.Core.Zones;

public delegate void ZoneEntered(KronstadtPlayer player, Zone zone);
public delegate void ZoneExited(KronstadtPlayer player, Zone zone);

public class ZoneManager
{
    public static ZoneEntered? OnZoneEntered;
    public static ZoneExited? OnZoneExited;

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
            LoadZone(zone);
        }

        ServerManager.OnServerSave += OnServerSave;
    }

    private static void OnServerSave()
    {
        string content = JsonConvert.SerializeObject(_Zones);

        using StreamWriter writer = new(File.Open(ZonesFile, FileMode.Create, FileAccess.Write));
        writer.Write(content);
    }

    private static void LoadZone(Zone zone)
    {
        GameObject zoneObject = new();
        ZoneComponent component = zoneObject.AddComponent<ZoneComponent>();
        component.Init(zone);

        SphereCollider collider = zoneObject.AddComponent<SphereCollider>();
        collider.radius = zone.Radius;
        collider.isTrigger = true;

        zoneObject.transform.SetPositionAndRotation(zone.Center.ToVector3(), Quaternion.Euler(0, 0, 0));
        _Zones.Add(zone);
        _Logger.LogDebug($"Added zone {zone.Center.ToVector3()}, radius {zone.Radius}");
    }

    public static void AddZone(Zone zone)
    {
        LoadZone(zone);
    }
}
