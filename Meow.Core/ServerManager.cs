using SDG.Unturned;
using Meow.Core.Chat;
using Meow.Core.Formatting;
using Meow.Core.Players;
using Meow.Core.Translations;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Text;

namespace Meow.Core;

public delegate void PreShutdown();
public delegate void ServerSave();

public class ServerManager
{
    public static PreShutdown? OnPreShutdown;
    public static ServerSave? OnServerSave;

    private static CancellationTokenSource? _Source;

    private static async UniTask Shutdown()
    {
        MeowPlayerManager.KickAll(TranslationList.ShutdownKick); 
        await UniTask.Yield();
        try
        {
            await DoSave();
            OnPreShutdown?.Invoke();
        }
        catch (Exception ex)
        {
            // Logger doesn't exist here
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ResetColor();
        }

        await UniTask.Yield();

        Provider.shutdown();
    }

    public static bool CancelShutdown()
    {
        if (_Source == null || _Source.IsCancellationRequested)
        {
            return false;
        }

        _Source.Cancel();
        return true;
    }

    // Save format
    // u16 - id
    // u16 - health
    // u64 - owner 
    // u64 - group 
    // Pos:
    // f32 - x
    // f32 - y
    // f32 - z
    // Rot:
    // f32 - x
    // f32 - y
    // f32 - z
    // Region:
    // u8 - x
    // u8 - y
    // State:
    // u16 - state length
    // u8[] - state array
    private const string BackupPath = "Barricades.bin";
    private static void WriteBarricadeBackup()
    {
        using BinaryWriter writer = new(File.Open(BackupPath, FileMode.Create, FileAccess.Write), Encoding.UTF8);

        BarricadeRegion[,] regions = BarricadeManager.regions;
        int length = Regions.WORLD_SIZE;

        for (byte x = 0; x < length; x++)
        for (byte y = 0; y < length; y++)
        foreach (BarricadeDrop drop in regions[x, y].drops)
        {
            BarricadeData data = drop.GetServersideData();
            writer.Write(data.barricade.asset.id);
            byte[] guid = data.barricade.asset.GUID.ToByteArray();
            for (int i = 0; i < guid.Length; i++)
            {
                writer.Write(guid[i]);
            }

            writer.Write(data.barricade.health);
            writer.Write(data.owner);
            writer.Write(data.group);

            writer.Write(data.point.x); writer.Write(data.point.y);
            writer.Write(data.point.z);

            Vector3 eulerRotation = data.rotation.eulerAngles;
            writer.Write(eulerRotation.x);
            writer.Write(eulerRotation.y);
            writer.Write(eulerRotation.z);

            writer.Write(x);
            writer.Write(y);
            
            writer.Write((ushort)data.barricade.state.Length);
            writer.Write(data.barricade.state);
        }

        writer.Flush();
    }

    private const int BarricadeSize = 
        (sizeof(byte) * 16) // guid
        + sizeof(ushort) // health
        + sizeof(ulong) // owner
        + sizeof(ulong) // group
        + (sizeof(float) * 3) // pos
        + (sizeof(float) * 3) // rot
        + (sizeof(byte) * 2) // x, y
        + sizeof(ushort); // state size

    private static unsafe Vector3 ReadVector3(byte* current)
    {
        float* cFloat = (float*)current;
        return new(*cFloat, *(cFloat + 1), *(cFloat + 2));
    }

    internal static unsafe void LoadBarricadeBackup()
    {
        BarricadeRegion[,] regions = (BarricadeRegion[,])BarricadeManager.regions.Clone();
        int length = Regions.WORLD_SIZE;

        for (byte x = 0; x < length; x++)
        for (byte y = 0; y < length; y++)
        foreach (BarricadeDrop drop in regions[x, y].drops)
        {
            BarricadeManager.destroyBarricade(drop, x, y, ushort.MaxValue);
        }

        using BinaryReader reader = new(File.Open(BackupPath, FileMode.Open, FileAccess.Read), Encoding.UTF8);

        byte* bufferPtr = stackalloc byte[BarricadeSize];
        Span<byte> buffer = new(bufferPtr, BarricadeSize);
        while (true)
        {
            if (reader.Read(buffer) == 0)
            {
                return;
            }

            byte* current = bufferPtr;
            Guid guid = new(new ReadOnlySpan<byte>(current, 16));
            current += 16;

            ushort health = *(ushort*)current;
            current += sizeof(ushort);

            ulong owner = *(ulong*)current;
            current += sizeof(ulong);

            ulong group = *(ulong*)current;
            current += sizeof(ulong);

            Vector3 pos = ReadVector3(current);
            current += sizeof(float) * 3;

            Quaternion rot = Quaternion.Euler(ReadVector3(current));
            current += sizeof(float) * 3;

            byte x = *current;
            byte y = *(current + 1);
            current += 2;

            ushort stateLen = *(ushort*)current;
            byte[] state = new byte[stateLen];
            if (stateLen != 0)
            {
                reader.Read(state, 0, stateLen);
            }

            ItemBarricadeAsset asset = Assets.find<ItemBarricadeAsset>(guid);
            Barricade barricade = new(asset, health, state);
            BarricadeManager.dropNonPlantedBarricade(barricade, pos, rot, owner, group);
        }
    }

    private static async UniTask DoSave()
    {
        await UniTask.Yield();
        OnServerSave?.Invoke();
        SaveManager.save();
        //WriteBarricadeBackup();
    }

    public static void StartSave()
    {
        DoSave().Forget();
    }

    public static void QueueShutdown(uint delay)
    {
        _Source = new();
        DoShutdown(delay, _Source.Token).Forget();
    }

    private static async UniTask DoShutdown(uint delay, CancellationToken token)
    {
        bool first = false;
        MeowChat.BroadcastMessage(TranslationList.ShutdownMessage, Formatter.FormatTime(delay));
        for (; delay > 0; delay--)
        {
            if (first)
            {
                switch (delay)
                {
                    case 60:
                    case 30:
                    case 10:
                    case < 5:
                        MeowChat.BroadcastMessage(TranslationList.ShutdownMessage, Formatter.FormatTime(delay));
                        break;
                }
            }

            await UniTask.Delay(1000);
            first = true;

            if (_Source?.IsCancellationRequested ?? true)
            {
                return;
            }
        }

        await UniTask.Yield();
        await Shutdown();
    }
}
