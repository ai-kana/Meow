using HarmonyLib;
using Meow.Core.Formatting;
using Meow.Core.Players;
using Meow.Core.Translations;
using SDG.Unturned;

namespace Meow.Core.Patches;

[HarmonyPatch]
internal static class GroupMessagePatch
{
    private static readonly Translation GroupLeaveSelf = new("GroupLeaveSelf");
    private static readonly Translation GroupLeaveOther = new("GroupLeaveOther");

    private static readonly Translation GroupLeavingSelf = new("GroupLeavingSelf");
    private static readonly Translation GroupLeavingOther = new("GroupLeavingOther");

    [HarmonyPatch(typeof(GroupManager), "alertGroupmatesTimer", typeof(Player), typeof(uint))]
    [HarmonyPrefix]
    public static bool AlertGroupmatesTimer(Player p, uint remainingSeconds) 
    {
        if (!MeowPlayerManager.TryGetPlayer(p, out MeowPlayer self))
        {
            return false;
        }

        TranslationPackage time = Formatter.FormatTime(remainingSeconds);
        self.SendMessage(GroupLeaveSelf, time);

        foreach (MeowPlayer player in MeowPlayerManager.Players) 
        {
            if (self.SteamID != player.SteamID && player.IsInSameGroup(self))
            {
                player.SendMessage(GroupLeaveOther, self.Name, time);
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(GroupManager), "alertGroupmatesLeft", typeof(Player))]
    [HarmonyPrefix]
    private static bool AlertGroupmatesLeft(Player uPlayer) 
    {
        if (!MeowPlayerManager.TryGetPlayer(uPlayer, out MeowPlayer self))
        {
            return false;
        }
            
        self.SendMessage(GroupLeaveSelf);

        foreach (MeowPlayer player in MeowPlayerManager.Players) 
        {
            if (self.SteamID != player.SteamID && player.IsInSameGroup(self))
            {
                player.SendMessage(GroupLeaveOther, self.Name);
            }
        }

        return false;
    }
}
