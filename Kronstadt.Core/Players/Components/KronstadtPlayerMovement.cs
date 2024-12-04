using SDG.Unturned;
using UnityEngine;

namespace Kronstadt.Core.Players.Components;

public class KronstadtPlayerMovement
{
    public readonly KronstadtPlayer Owner;
    public Vector3 Position => Owner.Player.transform.position;
    public Quaternion Rotation => Owner.Player.transform.rotation;

    private PlayerMovement _PlayerMovement => Owner.Player.movement;

    public bool IsFrozen => _PlayerMovement.pluginSpeedMultiplier == 0;
    
    public KronstadtPlayerMovement(KronstadtPlayer owner)
    {
        Owner = owner;
    }

    public void Teleport(Vector3 location, Quaternion rotation)
    {
        Owner.Player.teleportToLocationUnsafe(location + new Vector3(0f, 0.5f, 0f), rotation.eulerAngles.y);
    }

    public void Teleport(Vector3 location)
    {
        Teleport(location, Rotation);
    }

    public void Teleport(KronstadtPlayer player)
    {
        Teleport(player.Movement.Position, player.Movement.Rotation);
    }

    public void SetSpeed(float speed)
    {
        _PlayerMovement.sendPluginSpeedMultiplier(speed);
    }

    public void ChangeSpeed(float delta)
    {
        _PlayerMovement.sendPluginSpeedMultiplier(_PlayerMovement.pluginSpeedMultiplier + delta);
    }

    public void ResetSpeed()
    {
        _PlayerMovement.sendPluginSpeedMultiplier(1f);
    }

    public void SetJump(float jump)
    {
        _PlayerMovement.sendPluginJumpMultiplier(jump);
    }

    public void ChangeJump(float delta)
    {
        _PlayerMovement.sendPluginJumpMultiplier(_PlayerMovement.pluginSpeedMultiplier + delta);
    }

    public void ResetJump()
    {
        _PlayerMovement.sendPluginJumpMultiplier(1f);
    }

    public void SetGravity(float gravity)
    {
        _PlayerMovement.sendPluginGravityMultiplier(gravity);
    }

    public void ChangeGravity(float delta)
    {
        _PlayerMovement.sendPluginGravityMultiplier(_PlayerMovement.pluginSpeedMultiplier + delta);
    }

    public void ResetGravity()
    {
        _PlayerMovement.sendPluginGravityMultiplier(1f);
    }

    public void Freeze()
    {
        SetSpeed(0f);
        SetJump(0f);
        SetGravity(0f);
        _PlayerMovement.forceRemoveFromVehicle();
    }

    public void Unfreeze()
    {
        ResetSpeed();
        ResetJump();
        ResetGravity();
    }
}
