using Microsoft.Extensions.Logging;
using SDG.Unturned;
using UnityEngine;
using Meow.Core.Formatting;
using Meow.Core.Players;
using Meow.Core.Logging;
using Meow.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Meow.Core.Commands.Framework;
using Meow.Core.Roles;
using Meow.Core.Translations;
using Cysharp.Threading.Tasks;

namespace Meow.Core.Chat;

public class MeowChat
{
    private static readonly ILogger _Logger;
    private static float _LocalChatDistance;

    static MeowChat()
    {
        _Logger = LoggerProvider.CreateLogger<MeowChat>();
        ChatManager.onChatted += OnChatted;

        OnConfigurationReloaded();
        ConfigurationEvents.OnConfigurationReloaded += OnConfigurationReloaded;
    }

    private static void OnConfigurationReloaded()
    {
        float distance = MeowHost.Configuration.GetValue<float>("LocalChatDistance");
        _LocalChatDistance = distance * distance;
    }

    private static void SendLocal(MeowPlayer sender, string text)
    {
        string message = $"[{Formatter.RedColor.ColorText("L")}] {sender.Name}: {text}";
        _Logger.LogInformation($"[Local] {sender.LogName}: {text}");

        foreach (MeowPlayer player in MeowPlayerManager.Players)
        {
            if (Vector3.SqrMagnitude(player.Movement.Position - sender.Movement.Position) > _LocalChatDistance)
            {
                continue;
            }

            SendMessage(message, Color.white, sender.SteamPlayer, player.SteamPlayer, EChatMode.GROUP, null, true).Forget();
        }
    }

    private static void SendGroup(MeowPlayer sender, string text)
    {
        string message = $"[{Formatter.RedColor.ColorText("G")}] {sender.Name}: {text}";
        _Logger.LogInformation($"[Group] {sender.LogName}: {text}");
        foreach (MeowPlayer player in MeowPlayerManager.Players)
        {
            if (!player.SteamPlayer.isMemberOfSameGroupAs(sender.SteamPlayer))
            {
                continue;
            }

            SendMessage(message, Color.white, sender.SteamPlayer, player.SteamPlayer, EChatMode.GROUP, null, true).Forget();
        }
    }

    private static void SendGlobal(MeowPlayer sender, string text)
    {
        string message = $"{GetChatTag(sender)}{sender.Name}: {text}";
        _Logger.LogInformation($"{sender.LogName}: {text}");
        foreach (MeowPlayer player in MeowPlayerManager.Players)
        {
            SendMessage(message, Color.white, sender.SteamPlayer, player.SteamPlayer, EChatMode.GROUP, null, true).Forget();
        }
    }

    private static string GetChatTag(MeowPlayer player)
    {
        IEnumerable<string> roles = RoleManager.GetRoles(player.Roles.Roles)
            .Where(x => x.DutyOnly ? player.Administration.OnDuty : true && x.ChatTag != string.Empty)
            .Select(x => x.ChatTag);

        if (roles.Count() == 0)
        {
            return "";
        }

        return $"[{Formatter.FormatList(roles, " | ")}] ";
    }

    private static void OnChatted(SteamPlayer steamPlayer, EChatMode mode, ref Color chatted, ref bool isRich, string text, ref bool isVisible)
    {
        isVisible = false;

        MeowPlayerManager.TryGetPlayer(steamPlayer, out MeowPlayer player);

        if (text.StartsWith("/"))
        {
            CommandManager.ExecuteCommand(text, player);
            return;
        }

        if (player.Moderation.IsMuted && mode != EChatMode.GROUP)
        {

        }

        string message = text.Replace("<", "< ").Replace("< 3", "<3");

        switch (mode)
        {
            case EChatMode.SAY:
            case EChatMode.GLOBAL:
            case EChatMode.WELCOME:
                SendGlobal(player, message);
                return;
            case EChatMode.GROUP:
                SendGroup(player, message);
                return;
            case EChatMode.LOCAL:
                SendLocal(player, message);
                return;
        }
    }

    private static IEnumerable<MeowPlayer> GetStaffChatMembers()
    {
        foreach (MeowPlayer player in MeowPlayerManager.Players)
        {
            if (player.Permissions.HasPermission("staffchat"))
            {
                yield return player;
            }
        }
    }

    public static void SendStaffChat(string message, MeowPlayer sender)
    {
        foreach (MeowPlayer player in GetStaffChatMembers())
        {
            SendMessage(
                $"[{Formatter.RedColor.ColorText("SC")}] {player.Name}: " + message, 
                Color.white, 
                sender.SteamPlayer, 
                player.SteamPlayer, 
                useRichText:true).Forget();
        }
    }

    public static void BroadcastMessage(MeowPlayer player, string message, params object[] args)
    {
        _Logger.LogInformation(string.Format(message, args));
        Broadcast(player, Formatter.Format(message, args));
    }

    public static void BroadcastMessage(IEnumerable<MeowPlayer> players, string message, params object[] args)
    {
        _Logger.LogInformation(string.Format(message, args));
        Broadcast(players, Formatter.Format(message, args));
    }

    public static void BroadcastMessage(string message, params object[] args)
    {
        _Logger.LogInformation(string.Format(message, args));
        IEnumerable<MeowPlayer> players = MeowPlayerManager.Players;
        Broadcast(players, Formatter.Format(message, args));
    }

    public static void BroadcastMessage(MeowPlayer player, Translation translation, params object[] args)
    {
        _Logger.LogInformation(translation.Translate("English", args));
        Broadcast(player, translation, args);
    }

    public static void BroadcastMessage(IEnumerable<MeowPlayer> players, Translation translation, params object[] args)
    {
        _Logger.LogInformation(translation.Translate("English", args));
        Broadcast(players, translation, args);
    }

    public static void BroadcastMessage(Translation translation, params object[] args)
    {
        _Logger.LogInformation(translation.Translate("English", args));
        IEnumerable<MeowPlayer> players = MeowPlayerManager.Players;
        Broadcast(players, translation, args);
    }

    private static void Broadcast(IEnumerable<MeowPlayer> players, Translation translation, params object[] args)
    {
        foreach (MeowPlayer player in players)
        {
            Broadcast(player, translation, args);
        }
    }

    private static void Broadcast(MeowPlayer player, Translation translation, params object[] args)
    {
        string message = translation.Translate(player, args);
        SendMessage("<b>" + message, Color.white, null, player.SteamPlayer, EChatMode.GLOBAL, Formatter.ChatIconUrl, true).Forget();
    }

    private static void Broadcast(IEnumerable<MeowPlayer> players, string message)
    {
        foreach (MeowPlayer player in players)
        {
            Broadcast(player, message);
        }
    }

    private static void Broadcast(MeowPlayer player, string message)
    {
        SendMessage("<b>" + message, Color.white, null, player.SteamPlayer, EChatMode.GLOBAL, Formatter.ChatIconUrl, true).Forget();
    }

    private static async UniTask SendMessage(
            string message, 
            Color color, 
            SteamPlayer? sender = null, 
            SteamPlayer? reciever = null, 
            EChatMode mode = EChatMode.SAY, 
            string? icon = null, 
            bool useRichText = true)
    {
        await UniTask.Yield();
        ChatManager.serverSendMessage(message, color, sender, reciever, mode, icon, useRichText);
    }
    
    public static void SendPrivateMessage(MeowPlayer sender, MeowPlayer receiver, string text)
    {
        text = Formatter.RemoveRichText(text);
        string message = $"[{Formatter.RedColor.ColorText("PM")}] {sender.Name} -> {receiver.Name}: {text}";
        _Logger.LogInformation(message);

        SendMessage(message, Color.white, sender.SteamPlayer, receiver.SteamPlayer).Forget();
        SendMessage(message, Color.white, sender.SteamPlayer, sender.SteamPlayer).Forget();
    }
}
