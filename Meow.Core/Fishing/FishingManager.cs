using System.Reflection;
using HarmonyLib;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SDG.NetTransport; 
using SDG.Unturned;
using UnityEngine;
using Meow.Core.Players;
using Meow.Core.Translations;

namespace Meow.Core.Fishing;

public class FishingSkill
{
    [JsonIgnore]
    private const float _Slope = -0.5f;
    [JsonIgnore]
    private const float _HighY = 60;
    [JsonIgnore]
    private const float _LowY = 55;

    private static uint Clamp(uint value, uint l, uint h)
    {
        if (value < l)
        {
            return value;
        }

        if (value > h)
        {
            return value;
        }

        return value;
    }

    private static uint Cbrt(ulong v)
    {
        return (uint)Math.Floor(Math.Pow(v, (1f / 3f)));
    }

    [JsonIgnore]
    public uint Level => Clamp((uint)Cbrt(Experience), 0, 100);
    [JsonProperty]
    public ulong Experience {get; protected set;}

    // x = Level
    // 0.5x + 60
    public uint GetHigherTimeRange()
    {
        uint value = (uint)((_Slope * Level) + _HighY);
        return value;
    }
    // 0.5x + 55
    public uint GetLowerTimeRange()
    {
        uint value = (uint)((_Slope * Level) + _LowY);
        return value;
    }

    public void AddExperience(ulong delta)
    {
        Experience += delta;
    }
}

[HarmonyPatch]
internal class FishingManager
{
    private static string OnGetRewardErrorContext(UseableFisher instance) {
        return "fishing " + instance.player.equipment.asset?.FriendlyName + " reward";
    }

    private static readonly Translation FishCaught = new("FishCaught", "You caught a {0}! +{1} experience");
    private static readonly Translation LevelUp = new("LevelUp", "You are now fishing level {0}");
    private static void SendFishingReward(MeowPlayer player)
    {
        FishingSkill skill = player.FishingSkill;
        uint level = skill.Level;

        IEnumerable<LootItem> items = MeowHost.Configuration.GetSection("Fishing").Get<IEnumerable<LootItem>>() ?? throw new("Fishing section null");
        LootTable table = new(items, player.FishingSkill.Level);

        LootItem item = table.GetItem();
        player.Inventory.GiveItem(item.Id);
        skill.AddExperience(item.Xp);

        player.SendMessage(FishCaught, new Item(item.Id, true).GetAsset().FriendlyName, item.Xp);

        if (skill.Level > level)
        {
            player.SendMessage(LevelUp, level);
        }
    }

    // Reset luck fields
    private static FieldInfo LastLuckField = typeof(UseableFisher).GetField("lastLuck", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo LuckTimeField = typeof(UseableFisher).GetField("luckTime", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo HasSplashedField = typeof(UseableFisher).GetField("hasSplashed", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo HasTuggedField = typeof(UseableFisher).GetField("hasTugged", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly ClientInstanceMethod<float> SendLuckTime = ClientInstanceMethod<float>.Get(typeof(UseableFisher), "ReceiveLuckTime");

    [HarmonyPatch(typeof(UseableFisher), "resetLuck", new Type[0])]
    [HarmonyPrefix]
    private static bool ResetLuck(UseableFisher __instance)
    {
        if (!MeowPlayerManager.TryGetPlayer(__instance.player, out MeowPlayer player))
        {
            return false;
        }

        FishingSkill? skill = player.FishingSkill;

        LastLuckField.SetValue(__instance, Time.realtimeSinceStartup);
        LuckTimeField.SetValue(__instance, UnityEngine.Random.Range(skill.GetLowerTimeRange(), skill.GetHigherTimeRange()));
        //luckTime = UnityEngine.Random.Range(50.2f, 60.2f) - strengthMultiplier * 33.5f;
        SendLuckTime.Invoke(__instance.GetNetId(), ENetReliability.Reliable, __instance.channel.GetOwnerTransportConnection(), (float)LuckTimeField.GetValue(__instance));

        HasSplashedField.SetValue(__instance, false);
        HasTuggedField.SetValue(__instance, false);

        return false;
    }

    // Start primary fields
    private static FieldInfo IsFishingField = typeof(UseableFisher).GetField("isFishing", BindingFlags.Instance | BindingFlags.NonPublic); 
    private static FieldInfo StartedReelField = typeof(UseableFisher).GetField("startedReel", BindingFlags.Instance | BindingFlags.NonPublic); 
    private static FieldInfo IsReelingField = typeof(UseableFisher).GetField("isReeling", BindingFlags.Instance | BindingFlags.NonPublic); 
    private static FieldInfo IsCatchField = typeof(UseableFisher).GetField("isCatch", BindingFlags.Instance | BindingFlags.NonPublic); 
    private static FieldInfo IsStrengtheningField = typeof(UseableFisher).GetField("isStrengthening", BindingFlags.Instance | BindingFlags.NonPublic); 
    private static FieldInfo StrengthTimeField = typeof(UseableFisher).GetField("strengthTime", BindingFlags.Instance | BindingFlags.NonPublic); 
    private static FieldInfo StrengthMultiplierField = typeof(UseableFisher).GetField("strengthMultiplier", BindingFlags.Instance | BindingFlags.NonPublic); 
    private static MethodInfo ReelMethod = typeof(UseableFisher).GetMethod("reel", BindingFlags.Instance | BindingFlags.NonPublic); 
    private static readonly ClientInstanceMethod SendPlayReel = ClientInstanceMethod.Get(typeof(UseableFisher), "ReceivePlayReel");

    // Close ur eyes
    [HarmonyPatch(typeof(UseableFisher), "startPrimary", new Type[0])]
    [HarmonyPrefix]
    private static bool StartPrimary(UseableFisher __instance, ref bool __result)
    {
        if (__instance.player.equipment.isBusy)
        {
            __result = false;
            return false;
        }

        if ((bool)IsFishingField.GetValue(__instance))
        {
            IsFishingField.SetValue(__instance, false);
            __instance.player.equipment.isBusy = true;
            StartedReelField.SetValue(__instance, Time.realtimeSinceStartup);
            IsReelingField.SetValue(__instance, true);
            ReelMethod.Invoke(__instance, new object[0]);

            if ((bool)IsCatchField.GetValue(__instance))
            {
                IsCatchField.SetValue(__instance, false);
                if (!MeowPlayerManager.TryGetPlayer(__instance.player, out MeowPlayer player))
                {
                    throw new KeyNotFoundException("Player not found");
                }

                SendFishingReward(player);
                __instance.player.sendStat(EPlayerStat.FOUND_FISHES);
            }
            SendPlayReel.Invoke(__instance.GetNetId(), ENetReliability.Unreliable, __instance.channel.GatherRemoteClientConnectionsExcludingOwner());
            AlertTool.alert(__instance.transform.position, 8f);
        }
        else
        {
            IsStrengtheningField.SetValue(__instance, true);
            StrengthTimeField.SetValue(__instance, 0u);
            StrengthMultiplierField.SetValue(__instance, 0f);
        }
        __result = true;
        return false;
    }
}
