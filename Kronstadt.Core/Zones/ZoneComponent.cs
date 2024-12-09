using Kronstadt.Core.Players;
using SDG.Unturned;
using UnityEngine;

namespace Kronstadt.Core.Zones;

internal class ZoneComponent : MonoBehaviour
{
    private Zone _Owner = null!;
    public void Init(Zone owner)
    {
        _Owner = owner;
        DontDestroyOnLoad(this);
        gameObject.layer = LayerMasks.BARRICADE;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.TryGetComponent<Player>(out Player player))
        {
            return;
        }

        if (!KronstadtPlayerManager.TryGetPlayer(player, out KronstadtPlayer kPlayer))
        {
            return;
        }

        ZoneManager.OnZoneEntered?.Invoke(kPlayer, _Owner);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.TryGetComponent<Player>(out Player player))
        {
            return;
        }

        if (!KronstadtPlayerManager.TryGetPlayer(player, out KronstadtPlayer kPlayer))
        {
            return;
        }

        ZoneManager.OnZoneExited?.Invoke(kPlayer, _Owner);
    }
}
