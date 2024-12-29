using Cysharp.Threading.Tasks;
using Meow.Core.Commands.Framework;
using Meow.Core.Extensions;
using Meow.Core.Players;
using Meow.Core.Startup;
using SDG.Unturned;
using UnityEngine;

namespace Meow.Core.Binds;

[Startup]
public static class BindManager
{
    static BindManager()
    {
        PlayerInput.onPluginKeyTick += OnPluginKeyTicked;
        MeowPlayerManager.OnPlayerConnected += OnPlayerConnected;
        MeowPlayerManager.OnPlayerDisconnected += OnPlayerDisconncted;
    }

    private static Dictionary<MeowPlayer, float> LastExecution = new();
    private static void OnPlayerConnected(MeowPlayer player)
    {
        LastExecution.Add(player, 0);
    }

    private static void OnPlayerDisconncted(MeowPlayer player)
    {
        LastExecution.Remove(player);
    }

    private static void OnPluginKeyTicked(Player player, uint simulation, byte key, bool state)
    {
        if (!state)
        {
            return;
        }

        if (!MeowPlayerManager.TryGetPlayer(player, out MeowPlayer p))
        {
            return;
        }

        if (Time.time - LastExecution[p] < 0.5f)
        {
            return;
        }

        if (!p.SaveData.CustomBinds.TryGetValue(key, out string value))
        {
            return;
        }

        LastExecution[p] = Time.time;
        CommandManager.ExecuteCommand(value, p).Forget();
    }

    public static void SetBind(MeowPlayer player, byte number, string command)
    {
        number--;
        player.SaveData.CustomBinds.AddOrUpdate(number, command);
    }

    public static bool RemoveBind(MeowPlayer player, byte number)
    {
        number--;
        return player.SaveData.CustomBinds.Remove(number);
    }
}
