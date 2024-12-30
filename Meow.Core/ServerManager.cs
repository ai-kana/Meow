using SDG.Unturned;
using Meow.Core.Chat;
using Meow.Core.Formatting;
using Meow.Core.Players;
using Meow.Core.Translations;
using Cysharp.Threading.Tasks;
using Meow.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Meow.Core;

public delegate void PreShutdown();
public delegate void ServerSave();

public class ServerManager
{
    public static PreShutdown? OnPreShutdown;
    public static ServerSave? OnServerSave;

    private static CancellationTokenSource? _Source;
    private static readonly ILogger _Logger;

    static ServerManager()
    {
        _Logger = LoggerProvider.CreateLogger<ServerManager>();
    }

    private static void DoSave()
    {
        IEnumerable<ServerSave>? saves = OnServerSave?.GetInvocationList().Cast<ServerSave>() ?? null;
        if (saves == null)
        {
            return;
        }

        foreach (ServerSave save in saves)
        {
            try
            {
                save?.Invoke();
            }
            catch (Exception exception)
            {
                _Logger.LogError(exception, "Exception on saving");
            }
        }
    }

    private static async UniTask Shutdown()
    {
        DoSave();
        MeowPlayerManager.KickAll(TranslationList.ShutdownKick); 
        try
        {
            OnPreShutdown?.Invoke();
        }
        catch (Exception ex)
        {
            // Logger doesn't exist here
            Console.WriteLine("Failed to save");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ResetColor();
        }

        await UniTask.Yield();

        SaveManager.save();
        Provider.shutdown(0);
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

    public static void QueueShutdown(uint delay)
    {
        _Source = new();
        _ = DoShutdown(delay, _Source.Token);
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
