using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Action = System.Action;

namespace Meow.Core.Players.Components;

public class MeowPlayerLife
{
    private PlayerLife _Life => Owner.Player.life;
    private readonly MeowPlayer Owner;

    public Action? OnPlayerDied;

    public MeowPlayerLife(MeowPlayer owner)
    {
        Owner = owner;
        PlayerLife.onPlayerDied += PlayerDied;
    }

    ~MeowPlayerLife()
    {
        PlayerLife.onPlayerDied -= PlayerDied;
    }

    private void PlayerDied(PlayerLife sender, EDeathCause cause, ELimb limb, CSteamID instigator)
    {
        OnPlayerDied?.Invoke();
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
