using Microsoft.Extensions.Logging;
using SDG.Unturned;
using Steamworks;
using Meow.Core.Logging;
using Meow.Core.Translations;

namespace Meow.Core.Players;

internal struct ConsolePlayer : IPlayer
{
    public string Name => "Console";
    public string LogName => "Console";
    public CSteamID SteamID => Provider.server;

    public string Language => "English";

    private readonly ILogger _Logger;

    public ConsolePlayer()
    {
        _Logger = LoggerProvider.CreateLogger<ConsolePlayer>();
    }

    public void SendMessage(string format, params object[] args)
    {
        _Logger.LogInformation(format, args);
    }

    public void SendMessage(Translation translation, params object[] args)
    {
        SendMessage(translation.TranslateNoColor(Language, args));
    }
}
