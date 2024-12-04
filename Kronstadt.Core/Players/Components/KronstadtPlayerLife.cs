using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Kronstadt.Core.Players.Components;

public class KronstadtPlayerLife
{
    private PlayerLife _Life => Owner.Player.life;
    private readonly KronstadtPlayer Owner;

    public KronstadtPlayerLife(KronstadtPlayer owner)
    {
        Owner = owner;
    }

    public void Heal()
    {
        _Life.sendRevive();
    }

    public void Kill()
    {
        _Life.askDamage(byte.MaxValue, Vector3.up * 5, EDeathCause.KILL, ELimb.SKULL, new CSteamID(0UL), out _, false, ERagdollEffect.GOLD, true, true);
    }
}
