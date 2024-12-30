using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Meow.Core.Configuration;
using Meow.Core.Startup;
using Microsoft.Extensions.Configuration;

namespace Meow.Core.Chat
{
    [Startup]
    public class MeowMOTD
    {
        public static CancellationTokenSource TokenSource;
        private static string[]? MotdList = null;
        private static int MotdDelaySeconds;
        static MeowMOTD()
        {
            TokenSource = new CancellationTokenSource();

            OnConfigurationReloaded();
            ConfigurationEvents.OnConfigurationReloaded += OnConfigurationReloaded;
        }

        private static void OnConfigurationReloaded()
        {
            IConfigurationSection section = MeowHost.Configuration.GetSection("Motd");
            MotdList = section.GetValue<IEnumerable<string>>("Messages")?.ToArray() ?? null;
            MotdDelaySeconds = section.GetValue<int>("Delay");

            TokenSource.Cancel();
            TokenSource.Dispose();
            TokenSource = new CancellationTokenSource();

            if (MotdList == null)
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
                if (i == MotdList!.Length)
                {
                    i = 0;
                }

                await UniTask.Delay(MotdDelaySeconds * 1000, cancellationToken: token);
                MeowChat.BroadcastMessage(MotdList[i]);

                i++;
            }
        }
    }
}
