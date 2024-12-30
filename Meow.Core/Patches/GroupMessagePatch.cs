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
    public static bool AlertGroupmatesTimer(Player player, uint remainingSeconds) 
    {
        if (!MeowPlayerManager.TryGetPlayer(player, out MeowPlayer self))
        {
            return false;
        }

        TranslationPackage time = Formatter.FormatTime(remainingSeconds);
        self.SendMessage(GroupLeaveSelf, time);

        foreach (MeowPlayer p in MeowPlayerManager.Players) 
        {
            if (self.SteamID != p.SteamID && p.IsInSameGroup(self))
            {
                p.SendMessage(GroupLeaveOther, self.Name, time);
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(GroupManager), "alertGroupmatesLeft", typeof(Player))]
    [HarmonyPrefix]
    private static bool AlertGroupmatesLeft(Player player) 
    {
        if (!MeowPlayerManager.TryGetPlayer(player, out MeowPlayer self))
        {
            return false;
        }
            
        self.SendMessage(GroupLeaveSelf);

        foreach (MeowPlayer p in MeowPlayerManager.Players) 
        {
            if (self.SteamID != p.SteamID && p.IsInSameGroup(self))
            {
                p.SendMessage(GroupLeaveOther, self.Name);
            }
        }

        return false;
    }
}
