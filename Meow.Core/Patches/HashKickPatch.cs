using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Meow.Core.Startup;
using Microsoft.Extensions.Configuration;
using SDG.NetPak;
using SDG.NetTransport;
using SDG.Unturned;

namespace Meow.Core.Patches;

[Startup]
[HarmonyPatch]
internal static class HashKickPatch
{
    static HashKickPatch()
    {
        const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
        const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        Assembly assembly = typeof(Provider).Assembly;
        Type type = assembly.GetType("ClientAssetIntegrity");
        RuntimeHelpers.RunClassConstructor(type.TypeHandle);

        FieldInfo info = type.GetField("serverKnownMissingGuids", PrivateStatic);
        ServerKnownMissingGuids = (HashSet<Guid>)info.GetValue(null);

        ServerHashesField = typeof(MasterBundleConfig).GetField("serverHashes", PrivateInstance);
        GetPlatformHashesMethod = assembly.GetType("MasterBundleHash").GetMethod("GetPlatformHash", PublicInstance);

        Type steamPlayerType = typeof(SteamPlayer);
        ClientPlatformField = typeof(SteamPlayer).GetField("clientPlatform", PrivateInstance);
        ValidatedGuidsField = typeof(SteamPlayer).GetField("validatedGuids", PrivateInstance);

        SendKickForInvalidGuid = ClientStaticMethod<Guid>.Get(Assets.ReceiveKickForInvalidGuid);
    }

    private static HashSet<Guid> ServerKnownMissingGuids;
    private static readonly NetLength MAX_ASSETS = new NetLength(7u);

    private static readonly FieldInfo ValidatedGuidsField;

    private static readonly ClientStaticMethod<Guid> SendKickForInvalidGuid;

    private static readonly FieldInfo ServerHashesField;
    private static readonly MethodInfo GetPlatformHashesMethod;

    private static readonly FieldInfo ClientPlatformField;

    private static byte[] ClientHash = new byte[20];
    private static bool ReadMessage(ref bool __result, ITransportConnection transportConnection, NetPakReader reader)
    {
        SteamPlayer player = Provider.findPlayer(transportConnection);
        if (player == null)
        {
            return false;
        }

        if (!reader.ReadBits(MAX_ASSETS.bitCount, out var value))
        {
            Provider.kick(player.playerID.steamID, "ValidateAssets unable to read itemCountBits");
            return false;
        }

        int itemCount = (int)value;
        if (itemCount > MAX_ASSETS.value)
        {
            Provider.kick(player.playerID.steamID, "ValidateAssets invalid itemCount");
            return false;
        }

        itemCount++;
        if (!reader.ReadBits(itemCount, out var value2))
        {
            Provider.kick(player.playerID.steamID, "ValidateAssets unable to read hasHashFlags");
            return false;
        }

        for (int i = 0; i < itemCount; i++)
        {
            if (!reader.ReadGuid(out var value3))
            {
                Provider.kick(player.playerID.steamID, "ValidateAssets unable to read guid");
                break;
            }

            if (value3 == Guid.Empty)
            {
                Provider.kick(player.playerID.steamID, "ValidateAssets empty guid");
                break;
            }
    
            HashSet<Guid> validatedGuids = (HashSet<Guid>)ValidatedGuidsField.GetValue(player);
            if (!validatedGuids.Add(value3))
            {
                Provider.kick(player.playerID.steamID, "ValidateAssets duplicate guid");
                break;
            }

            bool flag = (value2 & (uint)(1 << i)) != 0;
            if (flag && !reader.ReadBytes(ClientHash))
            {
                Provider.kick(player.playerID.steamID, "ValidateAssets unable to read clientHash");
                break;
            }

            if (ServerKnownMissingGuids.Contains(value3))
            {
                continue;
            }

            Asset asset = Assets.find(value3);
            if (asset == null)
            {
                if ((bool)Assets.shouldLoadAnyAssets)
                {
                    UnturnedLog.info($"Kicking {transportConnection} for invalid file integrity request guid: {value3:N}");
                    SendKickForInvalidGuid.Invoke(ENetReliability.Reliable, transportConnection, value3);
                    Provider.dismiss(player.playerID.steamID);
                }
                break;
            }

            if (flag)
            {
                byte[] array = asset.hash;
                object? hashes = ServerHashesField.GetValue(asset.originMasterBundle);
                if (asset.originMasterBundle != null && hashes != null)
                {
                    EClientPlatform platform = (EClientPlatform)ClientPlatformField.GetValue(player);
                    byte[] platformHash = (byte[])GetPlatformHashesMethod.Invoke(hashes, [platform]);
                    if (platformHash != null)
                    {
                        array = Hash.combine(array, platformHash);
                    }
                }

                if (!Hash.verifyHash(ClientHash, array))
                {
                    UnturnedLog.info($"Kicking {transportConnection} for asset hash mismatch: \"{asset.FriendlyName}\" Type: {asset.GetTypeFriendlyName()} File: \"{asset.name}\" Id: {value3:N} Client: {Hash.toString(ClientHash)} Server: {Hash.toString(array)}");

                    string discordInvite = MeowHost.Configuration.GetValue<string>("DiscordInviteLink")!;
                    Provider.kick(player.playerID.steamID,
                    $"""
                    You have conflicting assets. 
                    To fix this close your game, add '-NoWorkshopSubscriptions' to your game's launch options and then restart it.
                    If you need more help join our discord: {discordInvite}
                    """);

                    break;
                }
            }
            else if (asset.hash != null && asset.hash.Length == 20)
            {
                Provider.kick(player.playerID.steamID, $"missing asset: \"{asset.FriendlyName}\" File: \"{asset.name}\" Id: {value3:N}");
                break;
            }
        }

        return false;
    }
}
