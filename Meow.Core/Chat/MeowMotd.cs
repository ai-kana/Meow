using Cysharp.Threading.Tasks;
using Meow.Core.Configuration;
using Meow.Core.Startup;
using Microsoft.Extensions.Configuration;

namespace Meow.Core.Chat;

[Startup]
public class MeowMotd
{
    public static CancellationTokenSource? TokenSource;

    private static string[]? Messages = null;
    private static int DelaySeconds;

    static MeowMotd()
    {
        OnConfigurationReloaded();
        ConfigurationEvents.OnConfigurationReloaded += OnConfigurationReloaded;
    }

    private static void OnConfigurationReloaded()
    {
        IConfigurationSection section = MeowHost.Configuration.GetSection("Motd");
        Messages = section.GetValue<IEnumerable<string>>("Messages")?.ToArray() ?? null;
        DelaySeconds = section.GetValue<int>("Delay");

        TokenSource?.Cancel();
        TokenSource?.Dispose();
        TokenSource = new CancellationTokenSource();

        if (Messages == null || Messages.Length == 0)
        {
            return;
        }

        SendMotd(TokenSource.Token).Forget();
    }

    private static async UniTask SendMotd(CancellationToken token)
    {
        int i = 0;
        while (true)
        {
            if (i == Messages!.Length)
            {
                i = 0;
            }

            await UniTask.Delay(DelaySeconds * 1000, cancellationToken: token);
            MeowChat.BroadcastMessage(Messages[i]);

            i++;
        }
    }
}
