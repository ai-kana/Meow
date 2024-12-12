using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Meow.Core.Players;
using Meow.Core.Translations;
using Meow.Core.Extensions;

namespace Meow.Unturnov.Doors;

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

        if (!MeowPlayerManager.TryGetPlayer(owner, out MeowPlayer player))
        {
            return;
        }

        UnturnovPlugin.DoorPositions.AddOrUpdate(player.SteamID, Vector3.zero);
    }

    private static readonly Translation DoorAlreadyPlaced = new("DoorAlreadyPlaced", "You already have a door placed");

    private static void OnDeployRequested(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
    {
        if (asset.build != EBuild.GATE)
        {
            return;
        }

        if (!MeowPlayerManager.TryGetPlayer(new CSteamID(owner), out MeowPlayer player))
        {
            return;
        }

        if (player.SaveData.Data.ContainsKey(DoorPositionKey))
        {
            player.SendMessage(DoorAlreadyPlaced);
            shouldAllow = false;
            return;
        }

        UnturnovPlugin.DoorPositions.AddOrUpdate(player.SteamID, point);
    }
}
