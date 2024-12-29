using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Extensions;
using Meow.Core.Startup;
using SDG.Unturned;
using UnityEngine;

namespace Meow.Core.Players.Components;

[Startup]
public class MeowPlayerInput
{
    public readonly MeowPlayer Owner;
    private float _LastCommand;

    internal MeowPlayerInput(MeowPlayer owner)
    {
        Owner = owner;
        _LastCommand = 0;
    }

    static MeowPlayerInput()
    {
        PlayerInput.onPluginKeyTick += OnPluginKeyTick;
    }

    private static void OnPluginKeyTick(Player player, uint simulation, byte key, bool state)
    {
        if (!state)
        {
            return;
        }
        
        if (!MeowPlayerManager.TryGetPlayer(player, out MeowPlayer p))
        {
            return;
        }

        p.Input.OnPluginKeyPressed(key);
    }

    private void OnPluginKeyPressed(byte key)
    {
        if (Time.time - _LastCommand < 0.5f)
        {
            return;
        }

        if (!Owner.SaveData.CustomBinds.TryGetValue(key, out string value))
        {
            return;
        }

        _LastCommand = Time.time;
        CommandManager.ExecuteCommand(value, Owner).Forget();
    }

    public void SetBind(byte number, string command)
    {
        number--;
        Owner.SaveData.CustomBinds.AddOrUpdate(number, command);
    }

    public bool RemoveBind(byte number)
    {
        number--;
        return Owner.SaveData.CustomBinds.Remove(number);
    }
}
