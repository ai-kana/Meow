using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Kronstadt.Core.Players;
using Kronstadt.Core.Translations;
using Kronstadt.Core.Extensions;

namespace Kronstadt.Unturnov.Doors;

internal static class DoorManager
{
    public static void Load()
    {
        BarricadeManager.onDeployBarricadeRequested += OnDeployRequested;
        BarricadeDrop.OnSalvageRequested_Global += OnSalvageRequested;
    }

    public const string DoorPositionKey = "DoorPosition";

    private static void OnSalvageRequested(BarricadeDrop barricade, SteamPlayer instigatorClient, ref bool shouldAllow)
    {
        if (barricade.asset.build != EBuild.GATE)
        {
            return;
        }

        CSteamID owner = instigatorClient.playerID.steamID;
        if (barricade.GetServersideData().owner != owner.m_SteamID)
        {
            return;
        }

        if (!KronstadtPlayerManager.TryGetPlayer(owner, out KronstadtPlayer player))
        {
            return;
        }

        player.SaveData.Data.Remove(DoorPositionKey);
    }

    private static readonly Translation DoorAlreadyPlaced = new("DoorAlreadyPlaced", "You already have a door placed");

    private static void OnDeployRequested(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
    {
        if (asset.build != EBuild.GATE)
        {
            return;
        }

        if (!KronstadtPlayerManager.TryGetPlayer(new CSteamID(owner), out KronstadtPlayer player))
        {
            return;
        }

        if (player.SaveData.Data.ContainsKey(DoorPositionKey))
        {
            player.SendMessage(DoorAlreadyPlaced);
            shouldAllow = false;
            return;
        }

        player.SaveData.Data.AddOrUpdate(DoorPositionKey, Position.FromVector3(point));
    }
}
